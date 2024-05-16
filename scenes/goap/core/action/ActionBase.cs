using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class ActionBase : Node
{
	[Signal] public delegate void ActionStartedEventHandler();
	[Signal] public delegate void ActionFailedEventHandler();
	[Signal] public delegate void ActionCanceledEventHandler();
	[Signal] public delegate void ActionSucceededEventHandler();
	
	
	[Export] protected Dictionary<StringName, int> Preconditions = new();
	[Export] protected Dictionary<StringName, int> Effects = new();
	[Export] public double BaseCost = 1.0;

	public EActionStatus Status { get; protected set; } = EActionStatus.Ready;

	public virtual void Start(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		Status = EActionStatus.Running;
		
		EmitSignal(SignalName.ActionStarted);
	}
	
	public virtual EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		return Status;
	}

	public virtual void Fail(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		Status = EActionStatus.Failed;
        EmitSignal(SignalName.ActionFailed);
	}

	public virtual void Cancel(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		Status = EActionStatus.Canceled;
        EmitSignal(SignalName.ActionCanceled);
	}
	
	public virtual void Succeed()
	{
		Status = EActionStatus.Succeeded;
        EmitSignal(SignalName.ActionSucceeded);
	}

	public virtual void Complete()
	{
		Free();
	}
	
	public virtual bool IsValid(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		return true;
	}

	public virtual double CalculateCost(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		return BaseCost;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, int> GetPreconditions(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		return Preconditions;
	}

	/// <summary>
	/// should return the state of the world after this action is successfully completed
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, int> GetEffects(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
	{
		return Effects;
	}
}
