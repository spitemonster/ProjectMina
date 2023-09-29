using Godot;
using ProjectMina;

namespace ProjectMina;

[GlobalClass]
public partial class LevelBase : Node3D
{
	[Export] protected NavigationRegion3D NavigationRegion;

	private Marker3D _playerStart;

	public override void _Ready()
	{
		if (GetNode("%PlayerStart") is Marker3D s)
		{
			_playerStart = s;
			Global.Data.Player.GlobalPosition = _playerStart.GlobalPosition;
			Global.Data.Player.GlobalRotation = _playerStart.GlobalRotation;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (NavigationRegion != null)
		{
			// NavigationRegion.BakeNavigationMesh(true);
		}
	}

	public NavigationRegion3D GetNavigationRegion() => NavigationRegion;
}