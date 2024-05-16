using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class AgentComponent : AIBrainComponent
{
	[Signal] public delegate void PlanCompletedEventHandler();
	[Signal] public delegate void ActionCompletedEventHandler();
	// goals are resources that allow for simple setup and tweaking in the editor
	[Export] public Array<GoalBase> Goals = new();
	// actions are scripts that are run by and on this agent
	public Array<ActionBase> Actions = new();
	
	[Export] public float PlanCheckFrequency = 1.0f;

	public Timer PlanTimer;
	public Plan CurrentPlan { get; protected set; }
	public ActionBase CurrentAction { get; protected set; }
	
	private int _currentPlanRequest;
	private GoalBase _currentHighestPriorityGoal;

	private Dictionary<StringName, int> AgentState = new();

	public Dictionary<StringName, int> GetAgentState()
	{
		return AgentState.Duplicate();
	}

	public GoalBase GetRandomGoal()
	{
		return Goals.PickRandom();
	}
	
	public Array<GoalBase> GetGoals(Dictionary<StringName, int> worldState)
	{
		return Goals;
	}

	public Array<ActionBase> GetActions(Dictionary<StringName, int> worldState)
	{
		return Actions;
	}

	public void ReceivePlan(Plan plan)
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
			if (child is ActionBase action)
			{
				Actions.Add(action);
			}
		}
		
		// Perception.CharacterEnteredLineOfSight += character =>
		// {
		// 	GD.Print("character entered line of sight: ", character.Name);
		// 	if (!(bool)Blackboard.GetValue("enemy_visible"))
		// 	{
		// 		switch (character.Faction)
		// 		{
		// 			case (CharacterFaction.Thief):
		// 				Blackboard.SetValue("enemy_visible", true);
		// 				Blackboard.SetValue("target_enemy", character);
		// 				break;
		// 		}
		// 	}
		// };
		//
		// Perception.CharacterExitedLineOfSight += character =>
		// {
		// 	if ((CharacterBase)Blackboard.GetValue("target_enemy") == character)
		// 	{
		// 		GD.Print("character exited line of sight: ", character.Name);
		// 	}
		//
		// 	if (Perception.VisibleCharacters.Count == 0)
		// 	{
		// 		Blackboard.SetValue("enemy_visible", false);
		// 		Blackboard.SetValue("target_enemy", default);
		// 	}
		// 	else
		// 	{
		// 		Blackboard.SetValue("target_enemy", Perception.GetNearestVisibleCharacter());
		// 	}
		// };
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

		if (CurrentPlan == null ||
		    CurrentPlan.Status == EPlanStatus.Canceled || 
		    CurrentPlan.Status == EPlanStatus.Failed || 
		    CurrentPlan.Status == EPlanStatus.Succeeded)
		{
			if (_currentPlanRequest == 0)
			{
				_currentHighestPriorityGoal ??= _GetCurrentHighestPriorityGoal();
				_RequestNewPlan(_currentHighestPriorityGoal);
			}
			
			return;
		}
		
		// when we complete an action also check to see if we should update our plan
		if (CurrentPlan.Goal.GoalName != _currentHighestPriorityGoal.GoalName)
		{
			CurrentPlan.Cancel();
			return;
		}

		if (CurrentAction == null ||
		    CurrentAction.Status == EActionStatus.Canceled ||
		    CurrentAction.Status == EActionStatus.Failed ||
		    CurrentAction.Status == EActionStatus.Succeeded)
		{
			CurrentAction = CurrentPlan.GetNextAction();
		}

		var worldState = GetAgentState();
		worldState.Merge(Planner.Instance.GetWorldState(), true);

		if (CurrentAction.Status == EActionStatus.Ready)
		{
			CurrentAction.Start(this, CurrentPlan.Goal, worldState);
		}

		EActionStatus result = CurrentAction.Run(this, CurrentPlan.Goal, worldState);
		
		switch (result)
		{
			case EActionStatus.Running:
				break;
			case EActionStatus.Failed:
				break;
			case EActionStatus.Succeeded:
				_CompleteAction(CurrentAction);
				break;
		}
	}

	private void _CompleteAction(ActionBase action)
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Action Completed: ", action.Name, "[/color]");
		}
		EmitSignal(SignalName.ActionCompleted);

		CurrentAction.Complete();
		
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

		CurrentPlan.Complete();
		CurrentPlan = null;
		EmitSignal(SignalName.PlanCompleted);
		_UpdateGoals();
	}

	private void _RequestNewPlan(GoalBase goal)
	{
		if (EnableDebug)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Requesting new plan for goal: ", goal.GoalName, "[/color]");
		}
		_currentPlanRequest = Planner.Instance.RequestPlan(this, goal);
	}
	
	private GoalBase _GetCurrentHighestPriorityGoal()
	{
		var localWs = GetAgentState();
		
		if (EnableDebug) {
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Getting Highest Priority Goal[/color]");
			GD.PrintRich("[color=magenta]", localWs, "[/color]");
		}
		
		Goals = new Array<GoalBase>(Goals.OrderByDescending(g => g.Priority(localWs)));
			
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
