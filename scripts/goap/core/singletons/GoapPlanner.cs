using System.Linq;
using Godot; 
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class GoapPlanner : Node
{
	private partial class PlanRequest: GodotObject
	{
		public GoapAgentComponent Agent;
		public GoapGoalBase Goal;
	}

	[Signal] public delegate void PlanRequestedEventHandler(GoapAgentComponent agent, int requestID);
	[Signal] public delegate void PlanRequestRevokedEventHandler(GoapAgentComponent agent);
	
	[Export] protected bool EnableDebug;
	public static GoapPlanner Instance { get; private set; }
	public Dictionary<StringName, Variant> WorldState { get; private set; } = new();
	private Dictionary<int, PlanRequest> _planRequestQueue = new();
	private int _currentRequestIdIncrementor = 0;

	
	public override void _Ready()
	{
	}

	public int RequestPlan(GoapAgentComponent agent, GoapGoalBase goal)
	{
		_currentRequestIdIncrementor++;
		PlanRequest plan = new() { Agent = agent, Goal = goal };
		_planRequestQueue.Add(_currentRequestIdIncrementor, plan);
		
		return _currentRequestIdIncrementor;
	}

	public bool RevokePlanRequest(int requestID)
	{
		return _planRequestQueue.Remove(requestID);
	}

	public override void _Process(double delta)
	{
		if (_planRequestQueue.Count > 0)
		{
			var plan = _planRequestQueue[_planRequestQueue.Keys.First()];
			
			_planRequestQueue.Remove(_planRequestQueue.Keys.First());

			Array<GoapActionBase> p = _BuildPlan(plan.Agent, plan.Goal);

			if (p.Count > 0)
			{
				GD.Print("have a plan");
				GD.Print(p);
			}
			else
			{
				GD.Print("don't have a plan");
			}
			
			plan.Agent.ReceivePlan(p);
			
			// need to manually free godot objets
			plan.Free();
		}
	}

	private Array<GoapActionBase> _BuildPlan(GoapAgentComponent agent, GoapGoalBase goal)
	{
		Array<GoapActionBase> plan = new();
		
		var agentState = agent.GetState();
		Dictionary<StringName, Variant> localWs = WorldState.Duplicate();
		localWs.Merge(agentState.Duplicate(), true);

		// if the goal was satisfied in the time it took to get the plan, return an empty plan and the agent will figure it out
		if (goal.Satisfied(localWs))
		{
			return plan;
		}

		if (agent.GetActions(localWs).Count < 1)
		{
			GD.Print("Agent no available actions");
			return plan;
		}
		
		GoapActionBase action = _FindAction(agent, goal, localWs);
		
		// Dictionary<StringName, Variant> localGoalWs = localWs.Duplicate();
		// localGoalWs[goal.GoalName] = goal.DesiredValue(localWs);
		// GoapGoalBase currentGoal = goal; 
		//
		// GD.Print(localWs);
		//
		// var planIterationCount = 0;
		//
		// while (currentGoal != null && planIterationCount < 5)
		// {
		// 	GoapActionBase action = _FindAction(agent, goal, localWs);
		// 	Dictionary<StringName, Variant> effect = action.GetEffects();
		// 	
		// 	plan.Insert(0, action);
		//
		// 	currentGoal = effect.First();
		// 	planIterationCount++;
		// 	
		// 	GD.Print("local world state after looking for action step: ", localWs);
		// 	GD.Print(planIterationCount);
		// }

		// if (_StatesAreEqual(localWs, localGoalWs))
		// {
		// 	
		// 	GD.Print("we're aligned");
		// }
		// else
		// {
		// 	// GD.Print("Local Worldstate: ", localWs);
		// 	// GD.Print("Local Goal Worldstate: ", localGoalWs);
		// 	GD.Print("we are not aligned");
		// }

		// return plan;
		return new();
	}

	private GoapActionBase _FindAction(GoapAgentComponent agent, GoapGoalBase goal, Dictionary<StringName, Variant> worldState)
	{
		var actions = agent.GetActions(worldState);
		GD.Print("target goal: ", goal.GoalName);
		GD.Print("target goal value: ", goal.DesiredValue(worldState));

		foreach (var action in actions)
		{
			GD.Print("    action name: ", action.Name);
			GD.Print("    action effects: ", action.GetEffects());
			GD.Print("    goal satisfied by action: ", goal.Satisfied(action.GetEffects()));
			GD.Print("    action is valid: ", action.IsValid(agent, worldState));
			
			if (goal.Satisfied(action.GetEffects()) && action.IsValid(agent, worldState))
			{
				GD.Print("seeking action and selected ", action.Name, " to satisfy ", goal.GoalName);
				return action;
			}
		}
		
		GD.Print("Could not find suitable action");
		
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

	public static bool VariantsEqual(Variant variantOne, Variant variantTwo)
	{
		switch (variantOne.VariantType)
			{
				case Variant.Type.NodePath:
					return variantOne.AsNodePath() == variantTwo.AsNodePath();
					break;
				case Variant.Type.Float:
					return variantOne.As<float>() == variantTwo.As<float>();
					break;
				case Variant.Type.Bool:
					return variantOne.AsBool() == variantTwo.AsBool();
					break;
				case Variant.Type.Vector3:
					return variantOne.AsVector3() == variantTwo.AsVector3();

					break;
				case Variant.Type.Int:
					return variantOne.As<int>() == variantTwo.As<int>();
					break;
				case Variant.Type.Array:
					return variantOne.AsStringArray() == variantTwo.AsStringArray();
					break;
				default:
					GD.PushError("goap planner states are equal comparison checking a variant type not accounted for in code");
					return false;
					break;
			}
	}
	
	private static bool _StatesAreEqual(Dictionary<StringName, Variant> stateOne, Dictionary<StringName, Variant> stateTwo)
	{
		if (stateOne.Count != stateTwo.Count)
		{
			GD.Print("states are not equal because the count is not the same");
			return false;
		} 
		
		foreach (var pair in stateOne)
		{
			if (!stateOne.TryGetValue(pair.Key, out Variant value1))
			{
				GD.Print("state one doesn't have a value for this key: ", pair.Key);
				return false;
			}
			
			if (!stateTwo.TryGetValue(pair.Key, out Variant value2))
			{
				GD.Print("State Two did not have: ", pair.Key);
				return false;
			}

			switch (value1.VariantType)
			{
				case Variant.Type.NodePath:
					if (value1.AsNodePath() != value2.AsNodePath())
					{
						// GD.Print(pair.Key, ": values are node paths and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are node paths and they ARE equal");
					break;
				case Variant.Type.Float:
					if (value1.As<float>() != value2.As<float>())
					{
						// GD.Print(pair.Key, ": values are floats and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are floats and they ARE equal");
					break;
				case Variant.Type.Bool:
					if (value1.AsBool() != value2.AsBool())
					{
						// GD.Print(pair.Key, ": values are bool and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are bool and they ARE equal");
					break;
				case Variant.Type.Vector3:
					if (value1.AsVector3() != value2.AsVector3())
					{
						// GD.Print(pair.Key, ": values are vec3s and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are vec3s and they ARE equal");
					break;
				case Variant.Type.Int:
					if (value1.As<int>() != value2.As<int>())
					{
						// GD.Print(pair.Key, ": values are integers and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are integers and they ARE equal");
					break;
				case Variant.Type.Array:
					if (value1.AsStringArray() != value2.AsStringArray())
					{
						// GD.Print(pair.Key, ": values are string arrays and they are not equal");
						return false;
					}
                    
                    // GD.Print(pair.Key, ": values are string arrays and they ARE equal");
					break;
				default:
					GD.PushError("goap planner states are equal comparison checking a variant type not accounted for in code");
					break;
			}
		}
		
		return true;
	}
}