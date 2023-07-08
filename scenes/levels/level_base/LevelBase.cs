using Godot;
using ProjectMina;

namespace ProjectMina;

[GlobalClass]
public partial class LevelBase : Node3D
{
	private Marker3D _playerStart;

	public override void _Ready()
	{
		if (GetNode("%PlayerStart") is Marker3D s)
		{
			_playerStart = s;
			Global.Data.Player.GlobalPosition = _playerStart.GlobalPosition;
		}
	}
}