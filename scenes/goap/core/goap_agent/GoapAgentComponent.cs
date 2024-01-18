using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = System.Array;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapAgentComponent : AIBrainComponent
{
	[Signal] public delegate void PlanCompletedEventHandler();
	[Signal] public delegate void ActionCompletedEventHandler();
	// goals are resources that allow for simple setup and tweaking in the editor
	[Export] public Array<GoapGoalBase> Goals = new();
	// actions are scripts that are run by and on this agent
	public Array<GoapActionBase> Actions = new();
	
	[Export] public float PlanCheckFrequency = 1.0f;

	public Timer PlanTimer;
	public GoapPlan CurrentPlan { get; private set; }
	
	private int _currentPlanRequest;

	private GoapActionBase _currentAction;

	private GoapGoalBase _currentHighestPriorityGoal;

	public Dictionary<StringName, Variant> GetAgentState()
	{
		return Blackboard.GetBlackboard().Duplicate();
	}

	public GoapGoalBase GetRandomGoal()
	{
		return Goals.PickRandom();
	}
	
	public Array<GoapGoalBase> GetGoals(Dictionary<StringName, Variant> worldState)
	{
		return Goals;
	}

	public Array<GoapActionBase> GetActions(Dictionary<StringName, Variant> worldState)
	{
		return Actions;
	}

	public void ReceivePlan(GoapPlan plan)
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]RECEIVED PLAN[/color]");
			foreach (var step in plan.Steps)
			{
			
				GD.PrintRich("[color=magenta]	", step.Name, "[/color]");
			}
		}
		_currentPlanRequest = 0;
		CurrentPlan = plan;
	}

	public override void _Ready()
	{
		base._Ready();

		PlanTimer = new()
		{
			WaitTime = PlanCheckFrequency,
			Autostart = true,
			OneShot = false
		};

		PlanTimer.Timeout += _UpdateGoals;
		AddChild(PlanTimer);

		foreach (var child in GetChildren())
		{
			if (child is GoapActionBase action)
			{
				Actions.Add(action);
			}
		}
		
		Perception.CharacterEnteredLineOfSight += character =>
		{
			GD.Print("character entered line of sight: ", character.Name);
			if (!(bool)Blackboard.GetValue("enemy_visible"))
			{
				switch (character.Faction)
				{
					case (CharacterFaction.Thief):
						Blackboard.SetValue("enemy_visible", true);
						Blackboard.SetValue("target_enemy", character);
						break;
				}
			}
		};

		Perception.CharacterExitedLineOfSight += character =>
		{
			if ((CharacterBase)Blackboard.GetValue("target_enemy") == character)
			{
				GD.Print("character exited line of sight: ", character.Name);
			}

			if (Perception.VisibleCharacters.Count == 0)
			{
				Blackboard.SetValue("enemy_visible", false);
				Blackboard.SetValue("target_enemy", default);
			}
			else
			{
				Blackboard.SetValue("target_enemy", Perception.GetNearestVisibleCharacter());
			}
		};
	}

	public override void _Process(double delta)
	{
		if (!Active || !ShouldProcess)
		{
			return;
		}

		if ((bool)Blackboard.GetValue("enemy_visible") && _currentHighestPriorityGoal.GoalName == "enemy_health")
		{
			var targetEnemy = (CharacterBase)Blackboard.GetValue("target_enemy");

			Blackboard.SetValue("target_movement_position", targetEnemy.GlobalPosition);
		}
		
		if (CurrentPlan == null)
		{
			if (_currentPlanRequest == 0)
			{
				_currentHighestPriorityGoal ??= _GetCurrentHighestPriorityGoal();

				_RequestNewPlan(_currentHighestPriorityGoal);
			}

			if (EnableDebug)
			{
				GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
				GD.PrintRich("[color=magenta]Plan Empty[/color]");
			}
			// wait for a new plan
			return;
		}
						
		// when we complete an action also check to see if we should update our plan
		if (CurrentPlan != null && CurrentPlan.Goal.GoalName != _currentHighestPriorityGoal.GoalName)
		{
			CurrentPlan.Status = GoapPlanStatus.Canceled;
			CurrentPlan = null;
			return;
		}

		if (_currentAction == null)
		{
			if (CurrentPlan.Steps.Count == 0)
			{
				if (EnableDebug)
				{
					GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
					GD.PrintRich("[color=magenta]Plan Steps Count == 0[/color]");
				}
				// plan has expired so we should just wait for a new plan
				return;
			}
			
			_currentAction = CurrentPlan.Steps.Dequeue();

			if (EnableDebug)
			{
				GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
				GD.PrintRich("[color=magenta]NEW ACTION[/color]");
				GD.PrintRich("[color=magenta]", _currentAction.Name, "[/color]");
			}
		}

		if (_currentAction.Status == ActionStatus.Failed || _currentAction.Status == ActionStatus.Succeeded && EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Current action status failed or succeeded[/color]");
			GD.PrintRich("[color=magenta]", _currentAction.Name, "[/color]");
			GD.PrintRich("[color=magenta]", _currentAction.Status, "[/color]");
			return;
		}
		
		var ws = GoapPlanner.Instance.WorldState.Duplicate();
		ws.Merge(GetAgentState(), true);
		
		ActionStatus result = _currentAction.Run(this, CurrentPlan.Goal, ws);

		if (EnableDebug)
		{
			GD.Print("ACTION RESULT: ", result);
			GD.Print(_currentAction.Name);	
		}
		
		switch (result)
		{
			case ActionStatus.Running:
				if (EnableDebug)
				{
					GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
					GD.PrintRich("[color=magenta]Current action status running[/color]");
					GD.PrintRich("[color=magenta]", _currentAction.Name, "[/color]");
				}
				break;
			case ActionStatus.Failed:
				break;
			case ActionStatus.Succeeded:
				_CompleteAction(_currentAction);
				
				break;
		}
	}

	private void _CompleteAction(GoapActionBase action)
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Action Completed: ", action.Name, "[/color]");
		}
		EmitSignal(SignalName.ActionCompleted);

		_currentAction.Complete();
		_currentAction = null;
		
		if (CurrentPlan.Steps.Count == 0)
		{
			_CompletePlan();
		}
	}

	private void _CompletePlan()
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Plan Completed[/color]");
		}
		EmitSignal(SignalName.PlanCompleted);
		CurrentPlan = null;
		_UpdateGoals();
	}

	private void _RequestNewPlan(GoapGoalBase goal)
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Requesting new plan for goal: ", goal.GoalName, "[/color]");
		}
		_currentPlanRequest = GoapPlanner.Instance.RequestPlan(this, goal);
	}
	
	private GoapGoalBase _GetCurrentHighestPriorityGoal()
	{
		var localWs = Blackboard.GetBlackboard();
		if (EnableDebug) {
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Getting Highest Priority Goal[/color]");
			GD.PrintRich("[color=magenta]", localWs, "[/color]");
		}
		Goals = new Array<GoapGoalBase>(Goals.OrderByDescending(g => g.Priority(localWs)));
			
		if (EnableDebug)
		{
			foreach (var goal in Goals)
			{
				GD.PrintRich("[color=red]GOAL NAME: ", goal.GoalName, ". GOAL PRIORITY: ", goal.Priority(localWs), "[/color]");
			}
			
			GD.PrintRich("[color=magenta]Highest Priority Goal: ", Goals[0].GoalName, "[/color]");
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		}
		return Goals[0];
	}

	private void _UpdateGoals()
	{
		_currentHighestPriorityGoal = _GetCurrentHighestPriorityGoal();
	}
}
