using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class ProjectileBase : RigidBody3D
{
	public Godot.Collections.Array<Node3D> Exclude = new();

	[Export]
	public HitboxComponent _hitbox;

	[Export]
	protected float Lifetime = 3.0f;

	[Export]
	protected Resource ImpactParticle;

	private ParticleSystem impactParticle;

	private Timer _killTimer;

	public override void _Ready()
	{

		_hitbox.CanHit = true;
		_hitbox.Exclude = Exclude;

		if (ImpactParticle != null)
		{
			PackedScene scene = GD.Load<PackedScene>(ImpactParticle.ResourcePath);

			if (scene.Instantiate() is ParticleSystem p)
			{
				impactParticle = p;
				GetTree().Root.AddChild(p);
			}
		}

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
				// GD.Print(node.Name);
				// if (impactParticle != null)
				// {
				// 	impactParticle.GlobalPosition = GlobalPosition;
				// 	impactParticle.Play();
				// 	impactParticle.Finished += () =>
				// 	{
				// 		impactParticle?.QueueFree();
				// 	};

				// 	impactParticle = null;
				// }

				QueueFree();
			}
		};

		_killTimer = new Timer()
		{
			WaitTime = Lifetime,
			OneShot = true,
			Autostart = true
		};

		_killTimer.Timeout += () =>
		{
			// impactParticle?.QueueFree();
			QueueFree();
		};
	}
}
