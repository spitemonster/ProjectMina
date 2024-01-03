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
	[Export] public Array<GoapGoalBase> Goals = new();
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

	public Dictionary<StringName, Variant> GetState()
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
		// PlanTimer.Start();
		//
		// PlanTimer.CallDeferred("Start");
	}

	public override void _Process(double delta)
	{
		if (CurrentPlan != null)
		{
			if (CurrentPlan.Steps.Count == 0)
			{
				_CompletePlan();
				return;
			}
			
			if (_currentAction == null || _currentAction.Status != GoapActionBase.ActionStatus.Running)
			{
				_currentAction = CurrentPlan.Steps.Dequeue();
			}
			
			if (_currentAction != null)
			{
				Dictionary<StringName, Variant> ws = GoapPlanner.Instance.WorldState;
				ws.Merge(GetState(), true);
		
				if (_currentAction.Run(this, CurrentPlan.Goal, ws) == GoapActionBase.ActionStatus.Succeeded)
				{
					_CompleteAction(_currentAction);
				}
			}
		}
	}

	private void _CompleteAction(GoapActionBase action)
	{
		EmitSignal(SignalName.ActionCompleted);
		_currentAction = null;
	}

	private void _CompletePlan()
	{
		GD.Print("plan completed");
		EmitSignal(SignalName.PlanCompleted);
		CurrentPlan = null;
	}

	private void _CheckPlanStatus()
	{
		if (CurrentPlan == null)
		{
			GD.Print("Requesting new plan!");
			_RequestNewPlan();
		}
		// if the agent doesn't already have a plan, request a new one
		// CallDeferred("_RequestNewPlan");
	}

	private void _RequestNewPlan()
	{
		GoapPlanner.Instance.RequestPlan(this, _GetCurrentHighestPriorityGoal());
	}
	
	private GoapGoalBase _GetCurrentHighestPriorityGoal()
	{
		GD.Print("getting current highest priority goal");
		Goals = new Array<GoapGoalBase>(Goals.OrderByDescending(g => g.Priority(Blackboard.GetBlackboard().Duplicate())));
		GD.Print(Goals);
		return Goals[0];
	}
}
