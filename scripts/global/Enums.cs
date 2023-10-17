using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class Enums : GodotObject
{

	public enum DirectionHorizontal
	{
		None,
		Left,
		Right
	}

	public enum DirectionVertical
	{
		None,
		Down,
		Up
	}
};