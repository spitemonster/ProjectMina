using Godot;
using System;
namespace ProjectMina;

public partial class RangedWeaponComponent : WeaponComponent
{
    [Signal] public delegate void FiredEventHandler(int remainingAmmoInClip, int remainingAmmoInReserve);
    [Signal] public delegate void ReloadedEventHandler(int ammoCount);
    
    [Export] protected RangedWeaponStats Stats;
    [Export] protected Marker3D ProjectileSpawnPoint;
    [Export] protected AnimationPlayer AnimPlayer;
    
    private RigidBody3D _projectile;
    private Vector3 _aimPosition;

    private int _maxAmmo = 0;
    public int CurrentAmmo { get; private set; } = 0;

    private bool _triggerPulled = false;

    private uint _defaultProjectileCollisionLayer;
    private uint _defaultProjectileCollisionMask;

    public void PullTrigger()
    {
        _triggerPulled = true;
        Attack();
    }

    public void ReleaseTrigger()
    {
        _triggerPulled = false;
        GD.Print("trigger released");
    }
    
    public void Aim(Vector3 aimPosition)
    {
        _aimPosition = aimPosition;
    }

    private void _SetCurrentAmmo(int amount = -1)
    {
        if (amount < 0)
        {
            CurrentAmmo = Stats.MaxAmmo;
        }
        else
        {
            CurrentAmmo = amount;
        }
    }

    protected override void Attack()
    {
        if (CurrentAmmo == 0)
        {
            return;
        }
        AnimPlayer.Stop();
        DisableUse();
        RigidBody3D proj = (RigidBody3D)_projectile.Duplicate(8);
        proj.Visible = true;
        proj.Freeze = false;
        
        Global.Data.CurrentLevel.AddChild(proj);

        var comp = proj.GetNodeOrNull<ProjectileComponent>("%Projectile");
        GD.Print("firing");

        if (comp == null)
        {
            GD.PushError("error firing bullet");
            return;
        }

        proj.CollisionLayer = _defaultProjectileCollisionLayer;
        proj.CollisionMask = _defaultProjectileCollisionMask;
        proj.GlobalTransform = ProjectileSpawnPoint.GlobalTransform;
        
        var dir = proj.GlobalPosition.DirectionTo(_aimPosition);

        // dir = Utilities.Math.GetRandomConeVector(dir, Stats.Spread);
        DebugDraw.Line(proj.GlobalPosition, proj.GlobalPosition + dir * 100f, Colors.Fuchsia);
        comp.Launch(dir);
        GD.Print("firing!");
        AnimPlayer?.Play("Fire");
        CurrentAmmo -= 1;
        EmitSignal(SignalName.Fired, CurrentAmmo, 56);
    }

    protected override void EndAttack()
    {
        GD.Print("should end attack");
        if (Stats.Automatic && _triggerPulled && CurrentAmmo > 0)
        {
            Attack();
        }
    }

    public virtual void Reload()
    {
        AnimPlayer?.Play("Reload");
    }

    public override void _Ready()
    {
        GD.Print("ready");
        base._Ready();
        WeaponType = EWeaponType.Ranged;
        
        _maxAmmo = Stats.MaxAmmo;
        CurrentAmmo = _maxAmmo;
    }

    public override void Equip(CharacterBase character, Node3D slot)
    {
        base.Equip(character, slot);
        _projectile = Stats.ProjectileScene.Instantiate<RigidBody3D>();
        _defaultProjectileCollisionLayer = _projectile.CollisionLayer;
        _defaultProjectileCollisionMask = _projectile.CollisionMask;
        _projectile.CollisionLayer = 0;
        _projectile.CollisionMask = 0;
        AddChild(_projectile);
        _projectile.Visible = false;
        _projectile.Position = ProjectileSpawnPoint.GlobalPosition;
        _projectile.Freeze = true;
    }
}
 