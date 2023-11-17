using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;
[Tool]
[GlobalClass]
public partial class Action : Node
{
	public virtual bool IsValid(Dictionary<string, Variant> worldState)
	{
		return true;
	}

	public virtual double CalculateCost(Dictionary<string, Variant> worldState)
	{
		return 1.0f;
	}

	public virtual Dictionary<string, Variant> GetPreconditions()
	{
		return new();
	}

	public virtual Dictionary<string, Variant> GetEffects()
	{
		return new();
	}
}
