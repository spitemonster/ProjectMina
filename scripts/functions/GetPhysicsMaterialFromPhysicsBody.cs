using Godot;

namespace ProjectMina;

public static partial class Utilities
{
    public static PhysicsMaterial GetPhysicsMaterialFromPhysicsBody(PhysicsBody3D physicsBody)
    {
        return physicsBody switch
        {
            StaticBody3D s => s.PhysicsMaterialOverride,
            RigidBody3D r => r.PhysicsMaterialOverride,
            _ => null
        };
    }
}