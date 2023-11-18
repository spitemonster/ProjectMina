using System.Collections.Generic;
using Godot;
using Godot.Collections;
using System.Diagnostics;
using System.Linq;
using Array = System.Array;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapAgentComponent : ComponentBase
{
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	[Export] public GoapGoalBase[] Goals;
	public Array<GoapAction> AvailableActions = new();
	public Array<GoapAction> CurrentPlan { get; private set; }

	private StringName[] _goals = new StringName[1];
	private int _goalCount;

	public Godot.Collections.Dictionary<StringName, Variant> GetState()
	{
		return Blackboard.GetBlackboard().Duplicate();
	}
	
	public Godot.Collections.Dictionary<StringName, Variant> GetGoals(Godot.Collections.Dictionary<StringName, Variant> worldState)
	{
		// A temporary list to hold goals with their priorities
		var prioritizedGoals = new List<(StringName GoalName, Variant Value, double Priority)>();

		foreach (var t in Goals)
		{
			if (Blackboard.HasValue(t.GoalName))
			{
				// Add goal along with its priority to the list
				prioritizedGoals.Add((t.GoalName, t.DesiredValue(worldState), t.Priority(worldState)));
			}
		}
		
		// Sort the list in descending order of priority
		prioritizedGoals.Sort((a, b) => b.Priority.CompareTo(a.Priority));

		Godot.Collections.Dictionary<StringName, Variant> goalDict = new();
		
		foreach (var t in Goals)
		{
			if (Blackboard.HasValue(t.GoalName))
			{
				goalDict.Add(t.GoalName, t.DesiredValue(worldState));
			}
		}

		return goalDict;
	}

	public void ReceivePlan(Array<GoapAction> plan)
	{
		
	}

	public override void _Ready()
	{
		_goalCount = Goals.Length;
		Array.Resize(ref _goals, _goalCount);

		for (var i = 0; i < _goalCount; i++)
		{
			if (Blackboard.HasValue(Goals[i].GoalName))
			{
				_goals[i] = Goals[i].GoalName;
			}
		}
	}

	public override void _Process(double delta)
	{
		GD.Print("goap agent is processing");
		GoapPlanner.Instance.RequestPlan(this);
		// GoapGoalBase currentGoal;
		// StringName currentGoalName;
		// for (var i = 0; i < _goalCount; i++)
		// {
		// 	currentGoalName = _goals[i];
		// 	GD.Print(Blackboard.GetValue(currentGoalName));
		// }
	}
}
