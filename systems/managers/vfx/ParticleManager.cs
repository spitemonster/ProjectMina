using Godot;
using Godot.Collections;

namespace ProjectMina;

public enum EParticleSystemType : uint
{
    Impact,
    MuzzleFlashPistol
}

public partial class ParticleManager : Node
{
    public static ParticleManager Instance;
    
    
    [ExportGroup("Particle Type")]
    [ExportSubgroup("Impact")]
    [Export] public PackedScene DefaultImpactParticle { get; protected set; }
    [Export] public Dictionary<StringName, Array<PackedScene>> ImpactParticles;
    // [Export] public Array<PackedScene> WoodImpactParticles { get; protected set; } = new();
    
    [ExportSubgroup("MuzzleFlash")]
    [Export] public PackedScene DefaultMuzzleFlashParticle { get; protected set; }
    [Export] public Dictionary<StringName, Array<PackedScene>> MuzzleFlashParticles;

    public static void SpawnParticleSystemAtPosition(Vector3 position, EParticleSystemType particleType, PhysicsMaterial material = null, Node3D parent = null)
    {
        ParticleSystemComponent system = null;
        
        switch (particleType)
        {
            case EParticleSystemType.Impact:
                system = GetImpactParticleSystem(material);
                break;
            case EParticleSystemType.MuzzleFlashPistol:
                system = GetMuzzleFlashParticleSystem();
                break;
        }

        if (system == null)
        {
            GD.PushError("No available particle system for selected type");
            return;
        }
        
        if (parent != null)
        {
            parent.AddChild(system);
            system.GlobalPosition = parent.GlobalPosition;
        }
        else
        {
            Global.Data.MainScene.AddChild(system);
            system.GlobalPosition = position;
        }
    }

    public static ParticleSystemComponent SpawnParticleSystem(EParticleSystemType particleType, PhysicsMaterial material = null)
    {
        ParticleSystemComponent system = null;
        
        switch (particleType)
        {
            case EParticleSystemType.Impact:
                system = GetImpactParticleSystem(material);
                break;
            case EParticleSystemType.MuzzleFlashPistol:
                system = GetMuzzleFlashParticleSystem();
                break;
        }

        return system;
    }

    public static void SpawnImpactParticleAtPosition(Vector3 position, PhysicsMaterial material = null)
    {
        var system = GetImpactParticleSystem(material);
        Global.Data.MainScene.AddChild(system);
        system.GlobalPosition = position;
        system.Play();
    }
    
    public static void SpawnMuzzleFlashParticleAtPosition(Vector3 position)
    {
        var system = GetMuzzleFlashParticleSystem();
        Global.Data.MainScene.AddChild(system);
        system.GlobalPosition = position;
        system.Play();
    }
    
    public static void SpawnMuzzleFlashParticleAtTransform(Transform3D transform)
    {
        var system = GetMuzzleFlashParticleSystem();
        Global.Data.MainScene.AddChild(system);
        system.GlobalTransform = transform;
        system.Play();
    }

    public static void SpawnMuzzleFlashParticleAsChild(Node3D parent)
    {
        var system = GetMuzzleFlashParticleSystem();
        parent.AddChild(system);
        system.GlobalPosition = parent.GlobalPosition;
        system.Play();
    }
    

    protected static ParticleSystemComponent GetDefaultImpactParticleSystem()
    {
        return Instance.DefaultImpactParticle.Instantiate<ParticleSystemComponent>();
    }
    
    protected static ParticleSystemComponent GetImpactParticleSystem(PhysicsMaterial material)
    {
        if (material == null)
        {
            return Instance.DefaultImpactParticle.Instantiate<ParticleSystemComponent>();
        }
        
        switch (Utilities.GetPhysicsMaterialType(material))
        {
            case EPhysicsMaterialType.None:
                return Instance.DefaultImpactParticle.Instantiate<ParticleSystemComponent>();
            case EPhysicsMaterialType.Wood:
                var rand = Instance.DefaultImpactParticle;
                return rand.Instantiate<ParticleSystemComponent>();
            default:
                return Instance.DefaultImpactParticle.Instantiate<ParticleSystemComponent>();
        }
    }

    protected static ParticleSystemComponent GetMuzzleFlashParticleSystem()
    {
        return Instance.DefaultMuzzleFlashParticle.Instantiate<ParticleSystemComponent>();
    } 

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }    
}
