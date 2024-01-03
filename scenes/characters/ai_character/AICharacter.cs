using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

public enum AIState
{
	Idle,
	Suspicious,
	Alerted
}

public enum AIBehavior
{
	Patrol,
	Search,
	Attack,
	Wait
}

[GlobalClass]
public partial class AICharacter : CharacterBase
{
	[Export] public AIBrainComponent Brain;
	[Export] public float BrakingDistance = 1.0f;
	[Export] public SearchComponent searchComponent { get; protected set; }
	[Export] protected AnimationPlayer _animPlayer;
	[Export] protected MeleeWeapon TempWeapon;
	[Export] protected AnimationTree _animationTree;
	[Export] protected BehaviorTreeComponent _behaviorTree;
	

	public Node3D _dtc;
	
	private Vector3 _direction = new();
	private Vector3 _lookTarget = new();

	public override void _Ready()
	{
		base._Ready();
		
		System.Diagnostics.Debug.Assert(Brain != null, "no brain component");
		System.Diagnostics.Debug.Assert(Brain.NavigationAgent != null, "No navigation agent");
		System.Diagnostics.Debug.Assert(CharacterMovement != null, "No movement component");
		
		Brain.NavigationAgent.DebugEnabled = true;
		
		CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			GD.Print("Character Health Changed");
		};

		CharacterMovement.EnableClimbing = false;
		CharacterMovement.EnableJumping = false;
		CharacterMovement.EnableSneaking = false;

		_dtc = GetNodeOrNull<Node3D>("%dtc");
		
		Brain.NavigationAgent.VelocityComputed += SetVelocity;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		for (var i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision3D = GetSlideCollision(i);
			if (collision3D.GetCollider() is not RigidBody3D r || !IsOnFloor()) continue;
			
			var directionToCollision = (collision3D.GetPosition() - CharacterBody.GlobalPosition).Normalized();
			var angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
			var angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
			angleFactor = Mathf.Round(angleFactor * 100) / 100;
		
			if (!(angleFactor > 0.5)) continue;
			
			r.ApplyCentralImpulse(-collision3D.GetNormal() * 4.0f * angleFactor);
			r.ApplyImpulse(-collision3D.GetNormal() * 0.01f * angleFactor, collision3D.GetPosition());
		}

		_direction = (Brain.NavigationAgent.TargetPosition - GlobalPosition).Normalized();
		_lookTarget = Brain.NavigationAgent.TargetPosition;
		
		var dist = (Brain.NavigationAgent.TargetPosition - GlobalPosition).Length() - .5f;
		var mult = Mathf.Clamp((dist / BrakingDistance), 0.0f, 1.0f);

		if (!Brain.NavigationAgent.IsTargetReached())
		{
			// Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, GetWorld3D().DirectSpaceState) * mult;
			Brain.NavigationAgent.Velocity = CharacterMovement.GetCharacterVelocity(_direction, delta, GetWorld3D().DirectSpaceState) * mult; 
		}
		else
		{
			// Velocity = CharacterMovement.GetCharacterVelocity(Vector3.Zero, delta, GetWorld3D().DirectSpaceState) * mult;
			Brain.NavigationAgent.Velocity = CharacterMovement.GetCharacterVelocity(Vector3.Zero, delta, GetWorld3D().DirectSpaceState) * mult;
		}
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		_animationTree.Set("parameters/test/blend_position", Velocity.Length());
		
		var t = GlobalTransform;
		t = t.InterpolateWith(t.LookingAt(_lookTarget, Vector3.Up), 0.05f);
		GlobalTransform = t;

		var gr = GlobalRotation;
		gr.X = 0;
		gr.Z = 0;
		GlobalRotation = gr;
		MoveAndSlide();
	}
}
