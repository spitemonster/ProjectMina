using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = System.Array;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapAgentComponent : ComponentBase
{
	[Signal] public delegate void PlanCompletedEventHandler();
	[Signal] public delegate void ActionCompletedEventHandler();
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	
	// goals are resources that allow for simple setup and tweaking in the editor
	[Export] public Array<GoapGoalBase> Goals = new();
	
	// actions are scripts that are run by and on this agent
	[Export] public Array<GoapActionBase> Actions = new();
	
	[Export] public float PlanCheckFrequency = 1.0f;
	
	// temporary for development
	[Export] public RigidBody3D Healing;
	[Export] public RigidBody3D Weapon;

	public Timer PlanTimer;
	public AICharacter Pawn { get; protected set; }
	public GoapPlan CurrentPlan { get; private set; }
	
	private int _currentPlanRequest;

	private GoapActionBase _currentAction;

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
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		GD.PrintRich("[color=magenta]RECEIVED PLAN[/color]");
		
		foreach (var step in plan.Steps)
		{
			GD.PrintRich("[color=magenta]	", step.Name, "[/color]");
		}
		_currentPlanRequest = 0;
		CurrentPlan = plan;
	}

	public override void _Ready()
	{
		if (GetOwner<AICharacter>() is { } c)
		{
			Pawn = c;
		}
		else
		{
			GD.PushError("Goap Agent not child of AICharacter");
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		PlanTimer = new()
		{
			WaitTime = PlanCheckFrequency,
			Autostart = true,
			OneShot = false
		};

		PlanTimer.Timeout += _CheckPlanStatus;
		AddChild(PlanTimer);
	}

	public override void _Process(double delta)
	{
		if (CurrentPlan == null)
		{
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]Plan Empty[/color]");
			// wait for a new plan
			return;
		}

		if (_currentAction == null)
		{
			if (CurrentPlan.Steps.Count == 0)
			{
				GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
				GD.PrintRich("[color=magenta]Plan Steps Count == 0[/color]");
				// plan has expired so we should just wait for a new plan
				return;
			}
			
			_currentAction = CurrentPlan.Steps.Dequeue();
			
			GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
			GD.PrintRich("[color=magenta]NEW ACTION[/color]");
			GD.PrintRich("[color=magenta]", _currentAction.Name, "[/color]");
		}

		if (_currentAction.Status == ActionStatus.Failed || _currentAction.Status == ActionStatus.Succeeded)
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

		switch (result)
		{
			case ActionStatus.Running:
				GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
				GD.PrintRich("[color=magenta]Current action status running[/color]");
				GD.PrintRich("[color=magenta]", _currentAction.Name, "[/color]");
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
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		GD.PrintRich("[color=magenta]Action Completed: ", action.Name, "[/color]");
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
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		GD.PrintRich("[color=magenta]Plan Completed[/color]");
		EmitSignal(SignalName.PlanCompleted);
		CurrentPlan = null;
	}

	private void _CheckPlanStatus()
	{
		if (CurrentPlan != null || _currentPlanRequest != 0) return;
		
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		GD.PrintRich("[color=magenta]REQUESTING A NEW PLAN BECAUSE CURRENT PLAN IS EMPTY[/color]");
		_RequestNewPlan();
	}

	private void _RequestNewPlan()
	{
		_currentPlanRequest = GoapPlanner.Instance.RequestPlan(this, _GetCurrentHighestPriorityGoal());
	}
	
	private GoapGoalBase _GetCurrentHighestPriorityGoal()
	{
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		GD.PrintRich("[color=magenta]Getting Highest Priority Goal[/color]");
		Goals = new Array<GoapGoalBase>(Goals.OrderByDescending(g => g.Priority(Blackboard.GetBlackboard().Duplicate())));
		GD.PrintRich("[color=magenta]Highest Priority Goal: ", Goals[0].GoalName, "[/color]");
		GD.PrintRich("[color=magenta]||=====GOAP AGENT=====||[/color]");
		return Goals[0];
	}
}
