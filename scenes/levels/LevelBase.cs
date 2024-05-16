using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class LevelBase : Node3D
{
	[Export] protected NavigationRegion3D NavigationRegion;
	[Export] public PackedScene PlayerClass { get; protected set; }
	[Export] public Marker3D PlayerStart { get; protected set; }

	public override void _Ready()
	{
		if (PlayerClass == null)
		{
			GD.PushError("No Player Class selected.");
			return;
		}

		if (GetNode("%PlayerStart") is Marker3D s)
		{
			PlayerStart = s;
		}
	}

	public NavigationRegion3D GetNavigationRegion() => NavigationRegion;
}
