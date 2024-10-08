using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class Target : Node3D
{
	private Area3D _bullseyeCollision;
	private Area3D _twentyCollision;
	private Area3D _tenCollision;
	private Area3D _fiveCollision;
	private Area3D _oneCollision;
	public override void _Ready()
	{
		_bullseyeCollision = GetNode<Area3D>("%BullseyeCollision");
		// _twentyCollision = GetNode<Area3D>("%TwentyCollision");
		// _tenCollision = GetNode<Area3D>("%TenCollision");
		// _fiveCollision = GetNode<Area3D>("%FiveCollision");
		// _oneCollision = GetNode<Area3D>("%OneCollision");

		if (_bullseyeCollision != null)
		{
			_bullseyeCollision.AreaEntered += (Area3D area) =>
			{
			};

			_bullseyeCollision.BodyEntered += (Node3D body) =>
			{
			};
		}
	}

	public override void _Process(double delta)
	{
	}
}
