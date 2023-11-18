using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapGoalBase : Resource
{
	[Export] public StringName GoalName { get; protected set; }
	[Export] public Array<Variant> BaseDesiredValue { get; protected set; } = new();
	[Export] public double BasePriority { get; protected set; } = 1.0;
	
	/// <summary>
	/// get the priority 
	/// </summary>
	/// <param name="worldState"></param>
	/// <param name="characterState"></param>
	/// <returns></returns>
	public virtual double Priority(Dictionary<StringName, Variant> worldState)
	{
		return BasePriority;
	}

	/// <summary>
	/// given a world state and character state, check if this goal is satisfied
	/// </summary>
	/// <param name="worldState"></param>
	/// <param name="characterState"></param>
	/// <returns></returns>
	public virtual bool Satisfied(WorldState worldState, WorldState characterState)
	{
		return false;
	}

	/// <summary>
	/// get the string name and value of the desired world state property of this goal
	/// world and character state are provided as options to allow for (more) complex desired state calculation
	/// i.e. always want more gold regardless of current amount
	/// </summary>
	/// <param name="worldState"></param>
	/// <param name="characterState"></param>
	/// <returns></returns>
	public virtual Variant DesiredValue(Dictionary<StringName, Variant> worldState)
	{
		return BaseDesiredValue[0];
	}
}