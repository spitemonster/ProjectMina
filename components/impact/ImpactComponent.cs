using Godot;

namespace ProjectMina;
public partial class ImpactComponent : ComponentBase3D
{
    public partial class ImpactResult : GodotObject
    {
        public float Force;
        public Vector3 Direction;
        public Vector3 Normal;
        public Vector3 Position;
    } 
    
    [Signal] public delegate void ImpactedEventHandler(ImpactResult impact);
    [Export] public float ImpactTimeoutDuration = .1f;
        
    private RigidBody3D _body;
    private float _previousVelocity = 0;
    private bool _canImpact = true;

    private Timer _impactTimeoutTimer;

    private void _HandleImpact(KinematicCollision3D collision)
    {
        // ImpactResult result = new()
        // {
        //     Force = 0.0f,
        //     Direction = Vector3.Zero,
        //     Normal = Vector3.Zero,
        //     Position = Vector3.Zero
        // };
        //
        // _DisableImpact();
        // _impactTimeoutTimer.Start();
        //
        // EmitSignal(SignalName.Impacted, result);
    }

    private void _DisableImpact()
    {
        _canImpact = false;
    }

    private void _EnableImpact()
    {
        _canImpact = true;
    }

    public override void _Ready()
    {
        _body = GetOwner<RigidBody3D>();

        _impactTimeoutTimer = new()
        {
            WaitTime = ImpactTimeoutDuration,
            OneShot = true,
            Autostart = false
        };

        _impactTimeoutTimer.Timeout += _EnableImpact;
        
        AddChild(_impactTimeoutTimer);

        if (_body == null)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _previousVelocity = _body.LinearVelocity.Length();

        KinematicCollision3D collision = _body.MoveAndCollide(_body.LinearVelocity, true);

        if (_previousVelocity > 1 && collision != null && _canImpact)
        {
            _HandleImpact(collision);
        }
    }
}
