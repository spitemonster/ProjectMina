using Godot;

namespace ProjectMina;

// this weapon type spawns a projectile and launches it at the target position
[GlobalClass]
public partial class RangedWeapon : WeaponBase
{
	public bool CanFire { get; private set; } = true;

	[Signal]
	public delegate void FiredEventHandler(); // fired when the weapon is fired
	[Signal]
	public delegate void AmmoDepletedEventHandler(); // fired when the weapon's clip has been emptied
	[Signal]
	public delegate void ReloadedEventHandler(); // fired when weapon has been reloaded
	[Signal]
	public delegate void ChamberedEventHandler(); // fired when weapon is ready to fire again

	[Export]
	protected float _fireDelay = 1.0f;
	[Export]
	protected Resource Projectile;
	[Export]
	protected Marker3D ProjectileSpawn;

	private PackedScene _projectileScene;
	private CharacterBase _owner;
	private Timer _fireTimer;

	public override void _Ready()
	{
		base._Ready();

		if (Projectile != null)
		{
			_projectileScene = GD.Load<PackedScene>(Projectile.ResourcePath);

			InteractionComponent.Used += Fire;
			EquipmentComponent.Equipped += Equip;
		}

		_fireTimer = new()
		{
			WaitTime = _fireDelay,
			OneShot = true,
			Autostart = false
		};

		AddChild(_fireTimer);

		_fireTimer.Timeout += Chamber;
	}

	public override void Equip(CharacterBase equippingCharacter)
	{
		base.Equip(equippingCharacter);
	}

	protected virtual void Fire(CharacterBase interactingCharacter)
	{
		if (!CanFire)
		{
			return;
		}

		ProjectileBase currentProjectile = GetProjectile();
		currentProjectile.ApplyImpulse(-GlobalTransform.Basis.Z * 100.0f);
		CanFire = false;
		_fireTimer.Start();
		EmitSignal(SignalName.Fired);
	}

	protected virtual void Chamber()
	{
		CanFire = true;
		EmitSignal(SignalName.Chambered);
	}

	protected ProjectileBase GetProjectile()
	{
		ProjectileBase currentProjectile = _projectileScene.Instantiate<ProjectileBase>();
		GetTree().Root.AddChild(currentProjectile);
		currentProjectile.GlobalTransform = ProjectileSpawn.GlobalTransform;
		currentProjectile.Exclude = Exclude;

		return currentProjectile;
	}
}
