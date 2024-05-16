using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoalBase : Resource
{
	[Export] public StringName GoalName { get; set; }
	// provided as an array here for better use and type setting in the editor
	[Export] public int BaseDesiredValue { get; set; }
	[Export] public double BasePriority { get; protected set; } = 1.0;
	
	/// <summary>
	/// get the priority. obvs virtual methods are intended to be overridden by the inheriting resources
	/// </summary>
	/// <param name="worldState"></param>
	/// <returns></returns>
	public virtual double Priority(Dictionary<StringName, int> worldState)
	{
		return BasePriority;
	}

	/// <summary>
	/// given a world state and character state, check if this goal is satisfied
	/// </summary>
	/// <param name="worldState"></param>
	/// <returns></returns>
	public virtual bool Satisfied(Dictionary<StringName, int> worldState)
	{
		if (!worldState.ContainsKey(GoalName))
		{
			return false;
		}

		return worldState[GoalName] == DesiredValue(worldState);
	}

	/// <summary>
	/// get the string name and value of the desired world state property of this goal
	/// world and character state are provided as options to allow for (more) complex desired state calculation
	/// i.e. always want more gold regardless of current amount
	/// </summary>
	/// <param name="worldState"></param>
	/// <returns></returns>
	public virtual int DesiredValue(Dictionary<StringName, int> worldState)
	{
		return BaseDesiredValue;
	}
}