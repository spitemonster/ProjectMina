using System;
using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class RangedWeaponComponent : WeaponComponent
{
    [Signal]
    public delegate void FiredEventHandler(int remainingAmmoInClip, int remainingAmmoInReserve);

    [Signal]
    public delegate void AmmoCountUpdatedEventHandler(int currentAmmo, int remainingAmmoInReserve);

    [Signal]
    public delegate void ReloadedEventHandler(int ammoCount);

    [Export] protected Marker3D ProjectileSpawn;
    [Export] protected AnimationPlayer AnimPlayer;
    [Export] protected AudioStreamPlayer3D AudioPlayer;

    public bool Firing { get; private set; } = false;
    public bool Reloading { get; private set; } = false;
    public bool CanFire { get; private set; } = false;
    public bool CanReload { get; private set; } = false;

    protected Vector3 ProjectileSpawnPosition => ProjectileSpawn?.GlobalPosition ?? Body.GlobalPosition;
    protected Transform3D ProjectileSpawnTransform => ProjectileSpawn?.GlobalTransform ?? Body.GlobalTransform;

    private RangedWeaponData _rangedWeaponData;

    private RigidBody3D _projectile;
    private Vector3 _aimPosition;

    private int _maxAmmo = 0;
    public int CurrentAmmo { get; private set; } = 0;

    private bool _triggerPulled = false;

    private uint _defaultProjectileCollisionLayer;
    private uint _defaultProjectileCollisionMask;

    private ParticleSystemComponent _muzzleFlashParticle;
    
    private double _fireRate => ((double)(1 / _rangedWeaponData.FireRate) * 60) / 60;

    private Timer _refireTimer;
    
    public void PullTrigger()
    {
        _triggerPulled = true;

        if (Firing || !CanUse)
        {
            return;
        }

        if (CurrentAmmo < 1)
        {
            CurrentAmmo = 0;
            AnimPlayer?.Play("Empty");
            return;
        }

        AnimPlayer?.Stop();

        if (!_rangedWeaponData.Automatic)
        {
            _refireTimer.Start();
        }

        Firing = true;
        DisableUse();
        AnimPlayer?.Play("Fire");
    }
    
    public void ReleaseTrigger()
    {
        _triggerPulled = false;

        if (_rangedWeaponData.Automatic)
        {
            AudioPlayer.Stop();
        }
        
        Refire();
    }

    public void LaunchProjectile()
    {
        // create projectile
        RigidBody3D proj = _rangedWeaponData.ProjectileScene.Instantiate<RigidBody3D>();
        
        // get projectile component
        var comp = proj.GetNodeOrNull<ProjectileComponent>("%Projectile");

        if (comp == null)
        {
            GD.PushError("Projectile does not have projectile component");
            return;
        }
        
        _SetCurrentAmmo(CurrentAmmo - 1);
        
        proj.Visible = true;
        proj.Freeze = false;
        
        comp.SetWeapon(this);
        
        Global.Data.CurrentLevel.AddChild(proj);
        proj.GlobalTransform = ProjectileSpawn.GlobalTransform;
        
        var dir = proj.GlobalPosition.DirectionTo(_aimPosition);
        dir = Utilities.Math.GetRandomConeVector(dir, _rangedWeaponData.Spread);
        
        comp.Launch(dir);
        EmitSignal(SignalName.Fired, CurrentAmmo, 56);
    }
    
    public void Refire()
    {
        Firing = false;
        EnableUse();
        
        if (CurrentAmmo == 0)
        {
            if (_rangedWeaponData.Automatic)
            {
                AudioPlayer.Stop();
            }

            return;
        }

        if (!_triggerPulled)
        {
            return;
        }
        
        if (_rangedWeaponData.Automatic)
        {
            LaunchProjectile();
            DisableUse();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Wielder == null)
        {
            return;
        }
    }
    
    public void Aim(Vector3 aimPosition)
    {
        _aimPosition = aimPosition;
    }

    private void _SetCurrentAmmo(int amount = -1)
    {
        if (CurrentAmmo == amount)
        {
            return;
        }
        
        GD.Print("should update ammo");
        
        CurrentAmmo = amount < 0 ? _rangedWeaponData.AmmoPerClip : amount;
        EmitSignal(SignalName.AmmoCountUpdated, CurrentAmmo);
    }
    
    public void PlayMuzzleFlash()
    {
        GD.Print("should muzzle flash");
        ParticleManager.SpawnMuzzleFlashParticleAtTransform(ProjectileSpawnTransform);
    }

    public void PlayFiringSound()
    {
        var stream = _rangedWeaponData.FiringSounds[0];
        AudioPlayer.SetStream(stream);
        AudioPlayer.Play();
    }

    public void PlayEmptySound()
    {
        if (AudioPlayer == null || _rangedWeaponData.EmptySounds.Length <= 0) return;
        
        AudioPlayer.SetStream(_rangedWeaponData.EmptySounds[0]);
        AudioPlayer.Play();
    }

    public virtual void Reload()
    {
        CurrentAmmo = _rangedWeaponData.AmmoPerClip;
        EmitSignal(SignalName.AmmoCountUpdated, CurrentAmmo, 56);
    }

    public override void _Ready()
    {
        _rangedWeaponData = (RangedWeaponData)Data;

        if (_rangedWeaponData == null)
        {
            GD.PushError("Problem!!!");
            return;
        }

        if (ProjectileSpawn == null)
        {
            GD.PushError("No projectile spawn position");
        }

        if (!_rangedWeaponData.Automatic)
        {
            _refireTimer = new()
            {
                WaitTime = _fireRate,
                OneShot = true,
                Autostart = false
            };
            _refireTimer.Timeout += Refire;
            AddChild(_refireTimer);
        }
        
        _maxAmmo = _rangedWeaponData.AmmoPerClip;
        CurrentAmmo = _maxAmmo;

        base._Ready();
    }
}
 