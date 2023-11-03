using Godot;
using Godot.Collections;
namespace ProjectMina.Goap;

[Tool]
[GlobalClass]
public partial class Planner : ComponentBase
{
	private static Planner _instance;
	public static Planner Instance => _instance;
	[Export] public static Array<Action> AvailableActions = new();
	[Export] public WorldState State = new();
	[Export] public BlackboardAsset Blackboard { get; protected set; }

	public Array<Action> GetPlan(Goal goal, Dictionary<string, Variant> characterState)
	{

		Dictionary<string, Variant> desiredState = goal.GetDesiredState();

		return _FindOptimalPlan(goal, desiredState, characterState);
	}

	public override void _Ready()
	{
		if (_debug)
		{
			Debug.Assert(Blackboard != null, "No blackboard asset on planner");
		}

		if (_instance != null)
		{
			QueueFree();
			return;
		}

		_instance = this;

		if (Blackboard != null)
		{
			State?.Initialize(Blackboard.Entries);
		}

	}

	private Array<Action> _FindOptimalPlan(Goal goal, Dictionary<string, Variant> desiredState, Dictionary<string, Variant> characterState)
	{


		Array<Action> plan = _BuildPlan(characterState, desiredState);

		return plan;
	}

	private Array<Action> _BuildPlan(Dictionary<string, Variant> characterState, Dictionary<string, Variant> desiredState)
	{

		return new();
	}
}

[GlobalClass]
public partial class Plan : GodotObject
{
	public Array<Action> Actions;
	public double Cost = 0.0f;

	public bool AddAction(Action action)
	{
		if (Actions.Contains(action))
		{
			return false;
		}

		Actions.Add(action);
		// Cost += action.CalculateCost(currentState);
		return true;
	}
}