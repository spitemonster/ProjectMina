using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;
[Tool]
[GlobalClass]
public abstract partial class Goal : GodotObject
{
	// this function receives the character and world states and determines if it is valid to run
	public virtual bool IsValid(WorldState characterState, WorldState worldState)
	{
		return true;
	}

	public virtual double Priority()
	{
		return 1.0;
	}

	public virtual Dictionary<string, Variant> GetDesiredState()
	{
		return new();
	}
}