using Godot;

namespace ProjectMina;
public partial class SteeringComponent : ComponentBase3D
{
    [Export] protected RayCast3D ForwardCast;
    [Export] protected RayCast3D LeftCast;
    [Export] protected RayCast3D LeftDiagonalCast;
    [Export] protected RayCast3D RightCast;
    [Export] protected RayCast3D RightDiagonalCast;
    public override void _Ready()
    {
    }

    public Vector3 CalculateSteeringVelocity(Vector3 velocity)
    {
        Vector3 steeringVelocity = velocity;
        Vector3 steering = Vector3.Zero;
        
        steering += GetSteeringVector(ForwardCast);
        steering += GetSteeringVector(LeftCast);
        steering += GetSteeringVector(RightCast);
        steering += GetSteeringVector(LeftDiagonalCast);
        steering += GetSteeringVector(RightDiagonalCast);

        steeringVelocity += steering;
        
        GD.Print("should calculate steering velocity");
        
        return steeringVelocity.Normalized() * velocity.Length();
    }

    private Vector3 GetSteeringVector(RayCast3D ray)
    {
        Vector3 direction = ray.TargetPosition.Normalized();
            
        if (!ray.IsColliding()) return Vector3.Zero;
        
        var length = (ray.GlobalPosition + ray.TargetPosition).Length();
        var distance = ray.GetCollisionPoint().DistanceTo(ray.GlobalPosition);
        var strength = Mathf.Clamp(1.0f - (distance / length), 0, 1);
        return -direction * strength; // Steer away from collision

    }
}
