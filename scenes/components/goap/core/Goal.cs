using System.Collections.Generic;
using Godot;

namespace ProjectMina.Goap;



[Tool]
[GlobalClass]
public abstract partial class Goal : Node
{
	public readonly short Weight;
	public readonly WorldState DesiredState;

	protected Goal(WorldState desiredState, short weight = 63)
	{
		Weight = weight;
		DesiredState = desiredState;
	}
}

[GlobalClass]
public partial class WorldProperty : GodotObject
{
	public readonly string Name;
	public readonly Variant Value;
}

[GlobalClass]
public partial class WorldState : GodotObject
{
	public readonly Godot.Collections.Array<WorldProperty> Properties;

	protected WorldState(Godot.Collections.Array<WorldProperty> properties)
	{
		Properties = properties;
	}
}