using System.Linq;
using Godot; 
using Godot.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProjectMina.Goap;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public partial class GoapPlanner : Node
{
	[Signal] public delegate void PlanRequestedEventHandler(GoapAgentComponent agent, int requestID);
	[Signal] public delegate void PlanRequestRevokedEventHandler(GoapAgentComponent agent);
	
	public static GoapPlanner Instance { get; private set; }
	public Godot.Collections.Dictionary<StringName, Variant> WorldState { get; private set; } = new();
	private Godot.Collections.Dictionary<int, PlanRequest> _planRequestQueue = new();
	private int _currentRequestIdIncrementor = 0;

	private GoapPlannerSettings _settings;
	private int _maxPlanSteps = 10;
	private bool _enableDebug = false;

	/// <summary>
	/// assign an id to a new plan request object that contains the goal and the requesting agent
	/// </summary>
	/// <param name="agent">the agent requesting the plan</param>
	/// <param name="goal">the goal for which we are building a plan</param>
	/// <returns>the plan request ID; this eventually becomes the plan id</returns>
	public int RequestPlan(GoapAgentComponent agent, GoapGoalBase goal)
	{
		// this value initializes at 0, incrementing at the start of the function ensures there is no plan request with the id 0
		_currentRequestIdIncrementor++;
		PlanRequest request = new() { ID = _currentRequestIdIncrementor, Agent = agent, Goal = goal };
		_planRequestQueue.Add(_currentRequestIdIncrementor, request);
		
		return _currentRequestIdIncrementor;
	}

	/// <summary>
	/// removes a plan request from the queue
	/// </summary>
	/// <param name="requestID">the request we're removing</param>
	/// <returns>whether or not the request was successfully removed</returns>
	public bool RevokePlanRequest(int requestID)
	{
		return _planRequestQueue.Remove(requestID);
	}

	public override void _Process(double delta)
	{
		if (_planRequestQueue.Count > 0)
		{
			var planRequest = _planRequestQueue.Values.First();
			
			_planRequestQueue.Remove(planRequest.ID);

			Queue<GoapActionBase> planQueue = _BuildPlan(planRequest.Agent, planRequest.Goal);

			if (planQueue.Count > 0)
			{
				GD.Print("have a plan request in the queue: ");
			}
			else
			{
				GD.Print("don't have any requests in the queue");
			}

			GoapPlan plan = new()
			{
				ID = planRequest.ID,
				Steps = planQueue,
				Goal = planRequest.Goal,
				Status = GoapPlanStatus.Ready
			};
			
			planRequest.Agent.ReceivePlan(plan);
			
			// need to manually free godot objets
			planRequest.Free();
		}
	}

	private Queue<GoapActionBase> _BuildPlan(GoapAgentComponent agent, GoapGoalBase primaryGoal)
	{
		Stack<GoapActionBase> plan = new();
		
		var agentState = agent.GetState();
		Godot.Collections.Dictionary<Godot.StringName, Godot.Variant> localWs = WorldState.Duplicate();
		localWs.Merge(agentState.Duplicate(), true);
		
		#if DEBUG
			if (primaryGoal.Satisfied(localWs))
			{
				if (_settings.EnableDebug)
				{
					GD.Print(agent.Name, "'s primary goal: ", primaryGoal.GoalName ," for their plan request has already been satisfied. Returning an empty plan.");
				}
			}
		
			if (agent.GetActions(localWs).Count < 1)
			{
				if (_settings.EnableDebug)
				{
					GD.Print(agent.Name, " has no available actions.");
				}
			}
		#endif

		// if the goal was satisfied in the time it took to get the plan or the agent has no available actionsZ, return an empty plan and the agent will figure it out
		if (primaryGoal.Satisfied(localWs) || agent.GetActions(localWs).Count < 1)
		{
			return new Queue<GoapActionBase>(plan);
		}
		
		var currentPlanStep = 0;
		var hasNextStep = true;
		var currentGoal = primaryGoal;
		
		while (hasNextStep && currentPlanStep < _maxPlanSteps)
		{
			// find action
			// update local world state
			GD.Print("CURRENT GOAL: ", currentGoal.GoalName, " IS NOT SATISFIED. SEARCHING FOR ACTION.");
			
			var action = _FindAction(agent, currentGoal, primaryGoal, localWs);
			
			GD.Print("selected action: ", action.Name);
			
			plan.Push(action);
			
			var actionEffects = action.GetEffects(agent, primaryGoal, localWs);
			
			localWs[actionEffects.Keys.First()] = actionEffects.Values.First();
			
			if (action.GetPreconditions(agent, primaryGoal, localWs).Keys.Count > 0)
			{
				GD.Print("action has precondition");
				
			
				currentGoal = new()
				{
					GoalName = action.GetPreconditions(agent, primaryGoal, localWs).Keys.First(),
					BaseDesiredValue = new() { action.GetPreconditions(agent, primaryGoal, localWs).Values.First() }
				};
				currentPlanStep++;	
			}
			else
			{
				GD.Print("action has no precondition");
				hasNextStep = false;
			}
		}
		
		GD.Print("GOAL FINALLY SATISFIED");
		GD.Print("========== plan ==========");
		GD.Print("goal: ", primaryGoal.GoalName);
		
		foreach (GoapActionBase step in plan)
		{
			GD.Print(step.Name);
		}
		
		GD.Print("========== plan ==========");

		return new(plan);
	}

	private GoapActionBase _FindAction(GoapAgentComponent agent, GoapGoalBase currentGoal, GoapGoalBase primaryGoal, Godot.Collections.Dictionary<StringName, Variant> worldState)
	{
		GD.Print("=========== FINDING ACTION ==========");
		var actions = agent.GetActions(worldState);
		GD.Print("target goal: ", currentGoal.GoalName);
		GD.Print("target goal value: ", currentGoal.DesiredValue(worldState));

		foreach (var action in actions)
		{
			GD.Print("    action name: ", action.Name);
			GD.Print("    action effects: ", action.GetEffects(agent, primaryGoal, worldState));
			GD.Print("    goal satisfied by action: ", currentGoal.Satisfied(action.GetEffects(agent, primaryGoal, worldState)));
			GD.Print("    action is valid: ", action.IsValid(agent, primaryGoal, worldState));
			
			// we are asking if
			// the current goal is satisfied by the outcome of the action given the primary goal
			if (currentGoal.Satisfied(action.GetEffects(agent, primaryGoal, worldState)) && action.IsValid(agent, primaryGoal, worldState))
			{
				GD.Print("    seeking action and selected ", action.Name, " to satisfy ", currentGoal.GoalName);
				GD.Print("=========== FOUND ACTION ==========");
				return action;
			}
		}
		
		GD.Print("Could not find suitable action");
		
		return null;
	}

	public static bool VariantsEqual(Variant variantOne, Variant variantTwo)
	{
		switch (variantOne.VariantType)
		{
			case Variant.Type.NodePath:
				return variantOne.AsNodePath() == variantTwo.AsNodePath();
			case Variant.Type.Float:
				return variantOne.As<float>() == variantTwo.As<float>();
			case Variant.Type.Bool:
				return variantOne.AsBool() == variantTwo.AsBool();
			case Variant.Type.Vector3:
				return variantOne.AsVector3() == variantTwo.AsVector3();
			case Variant.Type.Int:
				return variantOne.As<int>() == variantTwo.As<int>();
			case Variant.Type.Array:
				return variantOne.AsStringArray() == variantTwo.AsStringArray();
			default:
				GD.PushError("goap planner states are equal comparison checking a variant type not accounted for in code");
				return false;
		}
	}
	
	/// <summary>
	/// compare two states (Dictionary<StringName, Variant>) and determine if they are equal
	/// </summary>
	/// <param name="stateOne"></param>
	/// <param name="stateTwo"></param>
	/// <returns></returns>
	private static bool _StatesAreEqual(Godot.Collections.Dictionary<StringName, Variant> stateOne, Godot.Collections.Dictionary<StringName, Variant> stateTwo)
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

			if (!VariantsEqual(value1, value1))
			{
				return false;
			}
		}
		
		return true;
	}
	
	public override void _Ready()
	{
		// default settings exist but 
		_settings = ResourceLoader.Load<GoapPlannerSettings>("res://resources/settings/GoapPlannerSettings.tres");
		System.Diagnostics.Debug.Assert(_settings != null, "Goap planner singleton missing settings");

		if (_settings != null)
		{
			_maxPlanSteps = _settings.MaxPlanSteps;
			_enableDebug = _settings.EnableDebug;
		}
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