using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class GoapPlanner : Node
{
	[Export] protected bool EnableDebug;
	
	public static GoapPlanner Instance { get; private set; }
	// [Export] public static Array<GoapAction> AvailableActions = new();
	// [Export] public WorldState State = new();

	public Dictionary<StringName, Variant> WorldState = new();
	private Array<GoapAgentComponent> _planRequestQueue = new();

	public override void _Ready()
	{
	}

	public bool RequestPlan(GoapAgentComponent agent)
	{
		_planRequestQueue.Add(agent);
		return true;
	}

	public bool RevokePlanRequest(GoapAgentComponent agent)
	{
		if (!_planRequestQueue.Contains(agent)) return false;
		
		_planRequestQueue.Remove(agent);
		return true;

	}

	public override void _Process(double delta)
	{
		if (_planRequestQueue.Count > 0)
		{
			GoapAgentComponent agent = _planRequestQueue[0];
			agent.ReceivePlan(_BuildPlan(agent));
		}
	}

	// private Array<GoapAction> _FindOptimalPlan(Goal goal, Dictionary<string, Variant> desiredState, Dictionary<string, Variant> characterState)
	// {
	//
	//
	// 	Array<GoapAction> plan = _BuildPlan(characterState, desiredState);
	//
	// 	return plan;
	// }

	private Array<GoapAction> _BuildPlan(GoapAgentComponent agent)
	{
		var agentState = agent.GetState();
		var agentGoals = agent.GetGoals(WorldState);

		foreach (var goal in agentGoals)
		{
			if (agentState.ContainsKey(goal.Key) && !agentState[goal.Key].Equals(goal.Value))
			{
				GD.Print("goals and state do not align.");
				GD.Print("Goal ", goal.Key, ": ", goal.Value);
				GD.Print("State ", goal.Key, ": ", agentState[goal.Key]);
			}
		}
		
		
		return new();
	}

	public override void _EnterTree()
	{
		if (Instance != null)
		{
			QueueFree();
			return;
		}

		Instance = this;
	}
}
//
// [GlobalClass]
// public partial class Plan : GodotObject
// {
// 	public Array<Action> Actions;
// 	public double Cost = 0.0f;
//
// 	public bool AddAction(Action action)
// 	{
// 		if (Actions.Contains(action))
// 		{
// 			return false;
// 		}
//
// 		Actions.Add(action);
// 		// Cost += action.CalculateCost(currentState);
// 		return true;
// 	}
// }