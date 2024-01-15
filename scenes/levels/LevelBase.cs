using Godot;
using ProjectMina;

namespace ProjectMina;

[GlobalClass]
public partial class LevelBase : Node3D
{
	[Export] protected NavigationRegion3D NavigationRegion;
	[Export] public bool DevModeSpawn = false;
	[Export] protected PackedScene PlayerClass;

	private Marker3D _playerStart;

	public override void _Ready()
	{
		if (PlayerClass == null)
		{
			GD.PushError("No Player Class selected.");
			return;
		}
		
		if (GetNode("%PlayerStart") is Marker3D s)
		{
			_playerStart = s;

			if (PlayerClass.Instantiate() is CharacterBody3D p)
			{
				AddChild(p);
				p.GlobalTransform = _playerStart.GlobalTransform;
			}
			

			// Global.Data.Player.GlobalPosition = _playerStart.GlobalPosition;
			// Global.Data.Player.GlobalRotation = _playerStart.GlobalRotation;
		}
	}

	public NavigationRegion3D GetNavigationRegion() => NavigationRegion;
}
