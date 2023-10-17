using Godot;

namespace ProjectMina.Goap;

[Tool]
[GlobalClass]
public partial class WorldProperty : GodotObject
{
	[Export] public string Name;
	[Export] public Variant Value;

	public bool IsEqual(WorldProperty target)
	{
		return Name == target.Name && Value.Equals(target.Value);
	}

	public WorldProperty(string name, Variant value)
	{
		Name = name;
		Value = value;
	}
}
