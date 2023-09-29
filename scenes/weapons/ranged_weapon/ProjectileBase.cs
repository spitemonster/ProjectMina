using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class ProjectileBase : RigidBody3D
{
	public Godot.Collections.Array<Node3D> Exclude = new();

	public double Speed { get => Stats.Speed; }

	[Export] public HitboxComponent _hitbox;
	[Export] protected ProjectileStats Stats;
	[Export] protected bool ImpactParticleEnabled = true;
	[Export] protected bool ImpactSoundEnabled = true;
	[Export] protected bool TravelSoundEnabled = true;

	private AudioStreamPlayer3D _travelPlayer;

	private ParticleSystem impactParticle;

	private Timer _killTimer;

	public override void _Ready()
	{

		_hitbox.CanHit = true;
		_hitbox.Exclude = Exclude;

		if (Stats == null)
		{
			GD.PushError("Projectile missing Projectile Stats");
			return;
		}

		if (GetNode("%TravelPlayer") is AudioStreamPlayer3D player)
		{
			_travelPlayer = player;
		}

		if (Stats.ImpactParticle != null && ImpactParticleEnabled)
		{
			PackedScene scene = GD.Load<PackedScene>(Stats.ImpactParticle.ResourcePath);

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
				DebugDraw.Sphere(GlobalPosition, .1f, Colors.Blue, 10.0f);

				if (impactParticle != null)
				{
					impactParticle.GlobalPosition = GlobalPosition;

					impactParticle.Play();
					impactParticle.Finished += () =>
					{
						impactParticle?.QueueFree();
					};

					impactParticle = null;
				}

				QueueFree();
			}
		};

		_killTimer = new Timer()
		{
			WaitTime = Stats.Lifetime,
			OneShot = true,
			Autostart = true
		};

		_killTimer.Timeout += () =>
		{
			impactParticle?.QueueFree();
			QueueFree();
		};
	}
}
