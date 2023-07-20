using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class ProjectileBase : RigidBody3D
{
	public Godot.Collections.Array<Node3D> Exclude = new();

	[Export]
	protected HitboxComponent _hitbox;

	public override void _Ready()
	{
		_hitbox.CanHit = true;
		_hitbox.Exclude = Exclude;

		_hitbox.HitCharacter += (character) =>
		{
			if (!Exclude.Contains(character))
			{
				Visible = false;
			}

		};

		BodyEntered += (node) =>
		{
			if (!Exclude.Contains((Node3D)node))
			{
				QueueFree();
			}
		};
	}
}
