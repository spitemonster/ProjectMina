using System.Linq;
using Godot; 
using System.Collections.Generic;
using Godot.Collections;

namespace ProjectMina.Goap;

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

			GoapPlan plan = new()
			{
				ID = planRequest.ID,
				Steps = planQueue,
				Goal = planRequest.Goal,
				Status = GoapPlanStatus.Ready
			};
			
			planRequest.Agent.ReceivePlan(plan);
			
			// need to manually free godotobjets
			planRequest.Complete();
		}
	}

	private Queue<GoapActionBase> _BuildPlan(GoapAgentComponent agent, GoapGoalBase primaryGoal)
	{
		GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
		GD.PrintRich("[color=cyan]PLAN REQUEST GOAL: ", primaryGoal.GoalName, "[/color]");

		if (primaryGoal.GoalName == null)
		{
			return new Queue<GoapActionBase>();
		}
		
		Stack<GoapActionBase> plan = new();
		
		var agentState = agent.GetAgentState();
		
		// we don't want to touch our actual world state so we clone it and work with that for this process
		var localWs = WorldState.Duplicate();
		// merge our agent's state. agents and the world should never have the same states
		localWs.Merge(agentState.Duplicate(), true);
		
		GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
		GD.PrintRich("[color=cyan]ATTEMPTING TO BUILD PLAN WITH WORLD STATE[/color]");
		foreach (var state in localWs)
		{
			GD.PrintRich("[color=cyan]	", state.Key, ": ", state.Value, "[/color]");
		}

		// if the goal was satisfied in the time it took to get the plan or the agent has no available actions, return an empty plan and the agent will figure it out
		if (primaryGoal.Satisfied(localWs) || agent.GetActions(localWs).Count < 1)
		{
			GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
			GD.PrintRich("[color=cyan]returning empty goal because no actions or primary goal is satsified[/color]");
			return new Queue<GoapActionBase>(plan);
		}
		
		var currentPlanStep = 0;
		var hasNextStep = true;
		var currentGoal = primaryGoal;
		
		// outline of the below to make sure it makes sense to me
		// hasNextStep represents whether or not we have reached the conclusion of our plan;
		// i.e. whether or not the original goal for which we are building the plan is satisfied or not
		// _maxPlanSteps is basically just to cap how many levels deep the plan should get
		// using _FindAction gets an action for us that achieves our current goal
		while (hasNextStep && currentPlanStep < _maxPlanSteps)
		{
			var action = _FindAction(agent, currentGoal, primaryGoal, localWs);

			if (action == null)
			{
				GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
				GD.PrintRich("[color=cyan]returning empty goal because couldn't find an action[/color]");
				return new Queue<GoapActionBase>();
			}
			
			plan.Push(action);
			
			var actionEffects = action.GetEffects(agent, primaryGoal, localWs);

			// accommodate actions that have multiple effects. most do not and generally should not but I expect future me (hi!) will appreciate this
			foreach (var effect in actionEffects)
			{
				localWs[effect.Key] = effect.Value;
			}

			var preconditions = action.GetPreconditions(agent, primaryGoal, localWs);

			// if there are no preconditions, we're going to break out of the loop anyway
			if (preconditions.Count == 0)
			{
				hasNextStep = false;
				currentPlanStep++;
				continue;
			}

			var applicablePrecondition = _GetApplicablePrecondition(preconditions, localWs);

			if (applicablePrecondition == null)
			{
				hasNextStep = false;
			}
			else
			{
				currentGoal = applicablePrecondition;
			}
			
			currentPlanStep++;	
		}

		return new Queue<GoapActionBase>(plan);
	}

	private static GoapGoalBase _GetApplicablePrecondition(Godot.Collections.Dictionary<StringName, Variant> preconditions, Godot.Collections.Dictionary<StringName, Variant> worldState)
	{
		// assume all preconditions are satisfied until we're proven wrong just once
		var localGoal = new GoapGoalBase();
			
		foreach (var precondition in preconditions)
		{
			localGoal = new GoapGoalBase
			{
				GoalName = precondition.Key,
				BaseDesiredValue = new Array<Variant> { precondition.Value }
			};

			if (localGoal.Satisfied(worldState))
			{
				continue;
			}

			return localGoal;
		}
		
		return null;
	}

	private GoapActionBase _FindAction(GoapAgentComponent agent, GoapGoalBase currentGoal, GoapGoalBase primaryGoal, Godot.Collections.Dictionary<StringName, Variant> worldState)
	{
		var actions = agent.GetActions(worldState);

		foreach (var action in actions)
		{
			GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
			GD.PrintRich("[color=cyan]Finding Action[/color]");
			GD.PrintRich("	[color=cyan]", action.Name, "[/color]");
			GD.PrintRich("	[color=cyan]satisfied: ", currentGoal.Satisfied(action.GetEffects(agent, primaryGoal, worldState)), "[/color]");
			GD.PrintRich("	[color=cyan]valid: ", action.IsValid(agent, primaryGoal, worldState), "[/color]");
			// we are asking if
			// the current goal is satisfied by the outcome of the action given the primary goal
			if (currentGoal.Satisfied(action.GetEffects(agent, primaryGoal, worldState)) && action.IsValid(agent, primaryGoal, worldState))
			{
				return action;
			}
		}
		
		return null;
	}

	public static bool VariantsEqual(Variant variantOne, Variant variantTwo)
	{
		// return variantOne.Equals(variantTwo);
		
		switch (variantOne.VariantType)
		{
			case Variant.Type.Bool:
				return variantOne.AsBool().Equals(variantTwo.AsBool());
			case Variant.Type.NodePath:
				return variantOne.AsNodePath() == variantTwo.AsNodePath();
			case Variant.Type.Float:
				return variantOne.As<float>() == variantTwo.As<float>();
			case Variant.Type.Vector3:
				return variantOne.AsVector3() == variantTwo.AsVector3();
			case Variant.Type.Int:
				return variantOne.As<int>() == variantTwo.As<int>();
			case Variant.Type.Array:
				return variantOne.AsStringArray() == variantTwo.AsStringArray();
			default:
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
			return false;
		} 
		
		foreach (var pair in stateOne)
		{
			if (!stateOne.TryGetValue(pair.Key, out Variant value1))
			{
				return false;
			}
			
			if (!stateTwo.TryGetValue(pair.Key, out Variant value2))
			{
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