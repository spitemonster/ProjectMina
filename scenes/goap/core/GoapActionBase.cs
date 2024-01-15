using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public enum ActionStatus : uint
{
	Ready,
	Failed,
	Succeeded,
	Running
}

[GlobalClass]
public partial class GoapActionBase : Node
{
	[Export] protected Dictionary<StringName, Variant> Preconditions = new();
	[Export] protected Dictionary<StringName, Variant> Effects = new();
	[Export] public double BaseCost = 1.0;

	public ActionStatus Status = ActionStatus.Ready;
	
	
	
	public virtual bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
	{
		foreach (var condition in Preconditions)
		{
			Variant worldStateValue = worldState[condition.Key];
			
			if (!worldStateValue.Equals(condition.Value))
			{
				return false;
			}
		}
		
		return true;
	}

	public virtual double CalculateCost(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
	{
		return BaseCost;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, Variant> GetPreconditions(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
	{
		return Preconditions;
	}

	/// <summary>
	/// should return the state of the world after this action is successfully completed
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, Variant> GetEffects(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
	{
		return Effects;
		
		var localWs = worldState.Duplicate();
		foreach (var worldEffect in Effects)
		{
			if (localWs.ContainsKey(worldEffect.Key))
			{
				localWs[worldEffect.Key] = worldEffect.Value;
			}
		}

		return localWs;
	}

	public virtual void Complete()
	{
		Status = ActionStatus.Ready;
	}
	
	public virtual ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
	{
		Status = ActionStatus.Running;
		return Status;
	}
}
