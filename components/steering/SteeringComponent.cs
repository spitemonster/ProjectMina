using Godot;

namespace ProjectMina;

[GlobalClass,Icon("res://_dev/icons/icon--steering.svg")]
public partial class SteeringComponent : ComponentBase3D
{
    [Export] protected RayCast3D ForwardCast;
    [Export] protected RayCast3D LeftCast;
    [Export] protected RayCast3D LeftDiagonalCast;
    [Export] protected RayCast3D RightCast;
    [Export] protected RayCast3D RightDiagonalCast;

    [Export] public bool SteeringEnabled { get; protected set; } = true;

    private Vector3 _steeringVelocity = Vector3.Zero;
    
    public override void _Ready()
    {
    }

    // TODO: figure out why character steers into walls in hallways
    public Vector3 CalculateSteeringVelocity(Vector3 velocity)
    {
        if (!SteeringEnabled)
        {
            return velocity;
        }
        
        Vector3 steeringVelocity = velocity;
        Vector3 steering = Vector3.Zero;
        
        steering += GetSteeringVector(ForwardCast, 1f);
        steering += GetSteeringVector(LeftCast, .5f);
        steering += GetSteeringVector(RightCast, .5f);
        steering += GetSteeringVector(LeftDiagonalCast, .7f);
        steering += GetSteeringVector(RightDiagonalCast, .7f);
        
        // don't apply any steering velocity unless it exceeds a threshold
        if (steering.Length() < 0.01 || steering == Vector3.Zero)
        {
            return velocity;
        }

        steeringVelocity += steering;
        
        Vector3 start = new Vector3(GlobalPosition.X, 1.25f, GlobalPosition.Z);
        Vector3 end = (start + steeringVelocity);
        DebugDraw.Line(start, end, Colors.Cyan);

        return steeringVelocity.Normalized() * velocity.Length();
    }
    
    private bool _IsCollisionWalkable(Vector3 normal)
    {
        return normal.AngleTo(Vector3.Up) <= 45;
    }

    private Vector3 GetSteeringVector(RayCast3D ray, float weight)
    {
        Vector3 direction = ray.TargetPosition.Normalized();
            
        if (!ray.IsColliding() || _IsCollisionWalkable(ray.GetCollisionNormal())) return Vector3.Zero;
        
        var length = (ray.GlobalPosition + ray.TargetPosition).Length();
        var distance = ray.GetCollisionPoint().DistanceTo(ray.GlobalPosition);
        var strength = Mathf.Clamp(1.0f - (distance / length), 0, 1) * weight;
        return -direction * strength; // Steer away from collision

    }
}
