using Godot;
using System;
using Microsoft.VisualBasic.CompilerServices;

namespace ProjectMina;

public enum EProjectileType : uint
{
    None = 0,
    Handgun,
    Shotgun
}

public partial class ProjectileComponent : ComponentBase
{
    [Export] public ProjectileData Data;
    [Export] public HitboxComponent Hitbox;

    public RangedWeaponComponent Weapon;
    
    private RigidBody3D _body;

    private bool _launched = false;
    
    public virtual void Launch(Vector3 direction)
    {
        _launched = true;
        direction = direction.Normalized();
        _body.GlobalTransform = _body.GlobalTransform.LookingAt(_body.GlobalPosition + direction, Vector3.Up);
        _body.ApplyCentralImpulse(direction * Data.Speed);
        if (Hitbox == null)
        {
            GD.Print("FUCK");
        }
        Hitbox?.SetExclude(new()
        {
            // Weapon.Wielder.GetRid(),
            _body.GetRid()
        });
        Hitbox?.EnableHit();
    }

    public override void _Ready()
    {
        _body = GetOwner<RigidBody3D>();

        if (_body == null)
        {
            GD.PushError("projectile script attached to non rigidbody node");
            return;
        }
        
        _body.BodyEntered += _HandleCollision;
    }

    public bool SetWeapon(RangedWeaponComponent weapon)
    {
        if (Weapon != null)
        {
            return false;
        }

        Weapon = weapon;

        return true;
    }

    private void _HandleCollision(Node body)
    {
        if (body is not PhysicsBody3D or CsgShape3D)
        {
            return;
        }
        
        GD.Print("projectile hit: ", body.Name);

        HealthComponent healthComponent = null;

        if (body.GetNodeOrNull("%HealthComponent") is HealthComponent h)
        {
            healthComponent = h;
        } else if (body.GetOwner() != null && body.GetOwner().GetNodeOrNull("%HealthComponent") is HealthComponent hc)
        {
            healthComponent = hc;
        }

        healthComponent?.TakeDamage(Data.Damage);

        PhysicsMaterial physicsMaterial = null;
        if (body is PhysicsBody3D p)
        {
            physicsMaterial = Utilities.GetPhysicsMaterialFromPhysicsBody(p);
        }
            
        ParticleManager.SpawnImpactParticleAtPosition(_body.GlobalPosition, physicsMaterial);

        var t = new AudioStreamPlayer3D();
        Global.Data.MainScene.AddChild(t);
        t.SetStream(Data.ImpactSounds.PickRandom());
        t.Play();
        t.Finished += () =>
        {
            t.QueueFree();
        };
            
        _body.Visible = false;
        _body.QueueFree();
    }
}
