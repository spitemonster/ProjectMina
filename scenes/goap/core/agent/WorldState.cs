using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class WorldState : Resource
{
	[Export] public Dictionary<StringName, int> Properties { get; protected set; } = new();

	public int GetProperty(StringName propertyName)
	{
		return Properties[propertyName];
	}

	public bool HasProperty(StringName propertyName)
	{
		return Properties.ContainsKey(propertyName);
	}
}
