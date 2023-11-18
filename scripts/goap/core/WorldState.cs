using Godot;
using Godot.Collections;
using System.Linq;
namespace ProjectMina.Goap;

public partial class WorldState : GodotObject
{
	[Export] public Dictionary<StringName, Variant> Properties { get; protected set; } = new();

	public Variant GetProperty(StringName propertyName)
	{
		return Properties[propertyName];
	}

	public bool HasProperty(StringName propertyName)
	{
		return Properties.ContainsKey(propertyName);
	}

	public void Initialize(Dictionary<StringName, Variant> props)
	{
		Properties = props;
	}
}
