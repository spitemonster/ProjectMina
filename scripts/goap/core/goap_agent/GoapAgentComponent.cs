using System;
using Godot;
using Godot.Collections;
using Array = System.Array;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapAgentComponent : ComponentBase
{
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	[Export] public Array<GoapGoalBase> Goals = new();
	[Export] public Array<GoapActionBase> Actions = new();
	
	public AICharacter Pawn { get; protected set; }
	
	public Array<GoapActionBase> CurrentPlan { get; private set; } = new();

	private StringName[] _goals = new StringName[1];
	private int _currentPlanRequest;

	[Export] public RigidBody3D Healing;

	public Dictionary<StringName, Variant> GetState()
	{
		return Blackboard.GetBlackboard().Duplicate();
	}

	public GoapGoalBase GetCurrentHighestPriorityGoal()
	{
		return Goals[0];
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

	public void ReceivePlan(Array<GoapActionBase> plan)
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
		
		Array.Resize(ref _goals, Goals.Count);

		for (var i = 0; i < Goals.Count; i++)
		{
			if (Blackboard.HasValue(Goals[i].GoalName))
			{
				_goals[i] = Goals[i].GoalName;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (CurrentPlan.Count < 1)
		{
			_currentPlanRequest = GoapPlanner.Instance.RequestPlan(this, GetCurrentHighestPriorityGoal());
			return;
		}
		
		GD.Print("running action: ", CurrentPlan[0].Name);

		if (CurrentPlan[0].Run(this, GoapPlanner.Instance.WorldState) == GoapActionBase.ActionStatus.Succeeded)
		{
			CurrentPlan.RemoveAt(0);
			GD.Print("action completed successfully");
		}
	}

	private void _CompleteAction(GoapActionBase action)
	{
		
	}
}
