using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapActionBase : Node
{
	[Export] protected Dictionary<StringName, Variant> Preconditions = new();
	[Export] protected Dictionary<StringName, Variant> Effects = new();
	[Export] public double BaseCost = 1.0;
	
	public enum ActionStatus : uint
	{
		Failed,
		Succeeded,
		Running
	}
	
	public virtual bool IsValid(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
	{
		foreach (var condition in Preconditions)
		{
			Variant worldStateValue = worldState[condition.Key];

			GD.Print("attempting to check the validity of: ", Name, " - ", condition.Key);
			
			if (!GoapPlanner.VariantsEqual(worldStateValue, condition.Value)) ;
			{
				GD.Print("values do not match.");
				return false;
			}
		}
		
		return true;
	}

	public virtual double CalculateCost(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
	{
		return BaseCost;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, Variant> GetPreconditions()
	{
		return Preconditions;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public virtual Dictionary<StringName, Variant> GetEffects()
	{
		return Effects;
	}
	
	public virtual ActionStatus Run(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
	{
		return ActionStatus.Running;
	}
}
