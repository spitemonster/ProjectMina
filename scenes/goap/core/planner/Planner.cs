using Godot; 
using System.Collections.Generic;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class Planner : Node
{
	[Signal] public delegate void PlanRequestedEventHandler(AgentComponent agent, int requestID);
	[Signal] public delegate void PlanRequestRevokedEventHandler(AgentComponent agent);
	
	public static Planner Instance { get; private set; }
	public Godot.Collections.Dictionary<StringName, int> WorldState { get; private set; } = new();
	
	private Godot.Collections.Dictionary<int, PlanRequest> _planRequests = new();
	
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
	public int RequestPlan(AgentComponent agent, GoalBase goal)
	{
		// this value initializes at 0, incrementing at the start of the function ensures there is no plan request with the id 0
		_currentRequestIdIncrementor++;
		PlanRequest request = new() { ID = _currentRequestIdIncrementor, Agent = agent, Goal = goal };
		_planRequests.Add(request.ID, request);
		
		return _currentRequestIdIncrementor;
	}

	/// <summary>
	/// removes a plan request from the queue
	/// </summary>
	/// <param name="requestID">the request we're removing</param>
	/// <returns>whether or not the request was successfully removed</returns>
	public bool RevokePlanRequest(int requestID)
	{
		return _planRequests.Remove(requestID);
	}

	public Godot.Collections.Dictionary<StringName, int> GetWorldState()
	{
		return WorldState.Duplicate();
	}

	public override void _Process(double delta)
	{
		if (_planRequests.Count > 0)
		{
			PlanRequest planRequest = _planRequests[0];
			
			_planRequests.Remove(planRequest.ID);

			Plan plan = _BuildPlan(planRequest);
			
			planRequest.Agent.ReceivePlan(plan);
			planRequest.Fulfill();
		}
	}

	private Plan _BuildPlan(PlanRequest request)
	{
		Plan plan = new()
		{
			ID = request.ID,
			Goal = request.Goal
		};

		var worldState = request.Agent.GetAgentState();
		worldState.Merge(GetWorldState());

		int currentPlanStep = 0;
		bool hasNextStep = true;
		GoalBase currentGoal = request.Goal;

		Stack<ActionBase> planSteps = new();
		
		while (hasNextStep && currentPlanStep < _maxPlanSteps)
		{
			var action = _FindAction(request.Agent, currentGoal, request.Goal, worldState);
	
			if (action == null)
			{
				GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
				GD.PrintRich("[color=cyan]returning empty goal because couldn't find an action[/color]");
				break;
			}
			
			planSteps.Push(action);
			
			var actionEffects = action.GetEffects(request.Agent, request.Goal, worldState);
	
			// accommodate actions that have multiple effects. most do not and generally should not but I expect future me (hi!) will appreciate this
			foreach (var effect in actionEffects)
			{
				worldState[effect.Key] = effect.Value;
			}
	
			var preconditions = action.GetPreconditions(request.Agent, request.Goal, worldState);
	
			// if there are no preconditions, we're going to break out of the loop anyway
			if (preconditions.Count == 0)
			{
				hasNextStep = false;
				currentPlanStep++;
				continue;
			}
	
			var applicablePrecondition = _GetApplicablePrecondition(preconditions, worldState);
	
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

		plan.Steps = new(planSteps);
		plan.TotalSteps = planSteps.Count;

		return plan;
	}

	private static GoalBase _GetApplicablePrecondition(Godot.Collections.Dictionary<StringName, int> preconditions, Godot.Collections.Dictionary<StringName, int> worldState)
	{
		// assume all preconditions are satisfied until we're proven wrong just once
		foreach (var precondition in preconditions)
		{
			var localGoal = new GoalBase
			{
				GoalName = precondition.Key,
				BaseDesiredValue = precondition.Value
			};

			if (localGoal.Satisfied(worldState))
			{
				continue;
			}

			return localGoal;
		}
		
		return null;
	}

	private ActionBase _FindAction(AgentComponent agent, GoalBase currentGoal, GoalBase primaryGoal, Godot.Collections.Dictionary<StringName, int> worldState)
	{
		var actions = agent.GetActions(worldState);

		foreach (var action in actions)
		{
			// GD.PrintRich("[color=cyan]||==========GOAP PLANNER==========||[/color]");
			// GD.PrintRich("[color=cyan]Finding Action[/color]");
			// GD.PrintRich("	[color=cyan]", action.Name, "[/color]");
			// GD.PrintRich("	[color=cyan]satisfied: ", currentGoal.Satisfied(action.GetEffects(agent, primaryGoal, worldState)), "[/color]");
			// GD.PrintRich("	[color=cyan]valid: ", action.IsValid(agent, primaryGoal, worldState), "[/color]");
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
		_settings = ResourceLoader.Load<GoapPlannerSettings>("res://resources/settings/PlannerSettings.tres");
		
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