using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class LevelBase : Node3D
{
	[Export] public NavigationRegion3D NavigationRegion { get; protected set; }
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

		if (GetNodeOrNull("%NavigationRegion3D") is NavigationRegion3D n)
		{
			NavigationRegion = n;
		}
	}

	public NavigationRegion3D GetNavigationRegion() => NavigationRegion;
}
