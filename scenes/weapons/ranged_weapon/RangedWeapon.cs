using Godot;
using System;
using System.Diagnostics;
using System.IO;
namespace ProjectMina;

// this weapon type spawns a projectile and launches it at the target position
[GlobalClass]
public partial class RangedWeapon : WeaponBase
{
	public bool CanFire { get; private set; } = true;
	public int CurrentAmmo { get; private set; }

	[Signal] public delegate void FiredEventHandler(); // fired when the weapon is fired
	[Signal] public delegate void AmmoDepletedEventHandler(); // fired when the weapon's clip has been emptied
	[Signal] public delegate void ReloadedEventHandler(); // fired when weapon has been reloaded
	[Signal] public delegate void ChamberedEventHandler(); // fired when weapon is ready to fire again

	[Export] protected RangedWeaponStats WeaponStats;
	[Export] protected Marker3D ProjectileSpawn;
	[Export] protected SoundQueue3D SoundQueue;

	private PackedScene _projectileScene;
	private CharacterBase _owner;
	private Timer _fireTimer;
	private Timer _reloadTimer;

	private int _maxAmmo;
	private bool _automatic;
	private bool _triggerPulled;
	private Vector3 _aimPosition;
	private RandomNumberGenerator rng = new();

	private float _spreadRad;

	public override void _Ready()
	{
		base._Ready();

		Debug.Assert(WeaponStats != null, "No weapon stats");

		if (WeaponStats.Projectile != null)
		{
			_projectileScene = GD.Load<PackedScene>(WeaponStats.Projectile.ResourcePath);

			InteractionComponent.Used += PullTrigger;
			InteractionComponent.EndedUse += ReleaseTrigger;
			EquipmentComponent.Equipped += Equip;
		}

		_maxAmmo = WeaponStats.MaxAmmo;
		CurrentAmmo = _maxAmmo;

		_automatic = WeaponStats.Automatic;

		_fireTimer = new()
		{
			WaitTime = 1.0 / WeaponStats.FireRate,
			OneShot = true,
			Autostart = false
		};

		AddChild(_fireTimer);

		_fireTimer.Timeout += () =>
		{
			if (CurrentAmmo > 0)
			{
				Chamber();
			}

			if (_automatic && _triggerPulled)
			{
				Fire(_wieldingCharacter);
			}
		};

		_reloadTimer = new()
		{
			WaitTime = WeaponStats.ReloadDuration,
			OneShot = true,
			Autostart = false
		};

		AddChild(_reloadTimer);

		_reloadTimer.Timeout += () =>
		{
			CurrentAmmo = _maxAmmo;
			Chamber();
			EmitSignal(SignalName.Reloaded);

			if (SoundQueue != null && WeaponStats.ReloadSounds.Count > 0)
			{
				SoundQueue.PlaySound(WeaponStats.ReloadSounds.PickRandom());
			}
		};

		_spreadRad = WeaponStats.Spread;
	}

	public virtual void Aim(Vector3 AimPosition)
	{
		_aimPosition = AimPosition;
		// DebugDraw.Sphere(_aimPosition, .2f, Colors.Green, 3.0f);
	}

	public override void Equip(CharacterBase equippingCharacter)
	{
		base.Equip(equippingCharacter);
	}

	protected virtual void PullTrigger(CharacterBase interactingCharacter)
	{
		Fire(interactingCharacter);
		_triggerPulled = true;
	}

	protected virtual void ReleaseTrigger(CharacterBase interactingCharacter)
	{
		_triggerPulled = false;
	}


	protected virtual void Fire(CharacterBase interactingCharacter)
	{
		if (!CanFire)
		{
			return;
		}

		if (SoundQueue != null && WeaponStats.FireSounds.Count > 0)
		{
			AudioStream stream = (AudioStream)WeaponStats.FireSounds.PickRandom();
			SoundQueue.PlaySound(stream);
		}

		ProjectileBase currentProjectile = GetProjectile();
		currentProjectile.GlobalTransform = ProjectileSpawn.GlobalTransform;
		currentProjectile.LookAt(_aimPosition, Vector3.Up);

		DebugDraw.Line(currentProjectile.GlobalPosition, _aimPosition, Colors.Purple, 10.0f);
		DebugDraw.Sphere(_aimPosition, .25f, Colors.Purple, 10.0f);
		Vector3 ProjectileDirection = (_aimPosition - currentProjectile.GlobalPosition).Normalized();
		float randHOffset = ((rng.Randf() - .5f) * 2) * _spreadRad * (Mathf.Pi / 180);
		float randVOffset = ((rng.Randf() - .5f) * 2) * _spreadRad * (Mathf.Pi / 180);

		currentProjectile.Rotate(Vector3.Up, randHOffset);
		currentProjectile.Rotate(currentProjectile.GlobalTransform.Basis.X, randVOffset);

		currentProjectile.ApplyImpulse(ProjectileDirection * (float)currentProjectile.Speed);

		CanFire = false;

		if (CurrentAmmo > 0)
		{
			CurrentAmmo -= 1;
		}

		_fireTimer.Start();
		EmitSignal(SignalName.Fired);
	}

	public virtual void Reload()
	{
		_reloadTimer.Start();
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
		currentProjectile.TopLevel = true;
		currentProjectile.Exclude = Exclude;
		return currentProjectile;
	}
}
