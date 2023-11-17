using Godot;
using Godot.Collections;
using System.Linq;
namespace ProjectMina.Goap;

public partial class WorldState : GodotObject
{
	[Export] public Dictionary<string, Variant> Properties { get; protected set; } = new();

	public Variant GetProperty(string propertyName)
	{
		return Properties[propertyName];
	}

	public bool HasProperty(string propertyName)
	{
		return Properties.ContainsKey(propertyName);
	}

	public void Initialize(Dictionary<string, Variant> props)
	{
		Properties = props;
	}
}
