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
	[Export] public MovementComponent MovementComponent { get; protected set; }
	[Export] public SearchComponent searchComponent { get; protected set; }
	[Export] protected AnimationPlayer _animPlayer;
	[Export] protected MeleeWeapon TempWeapon;
	[Export] protected AnimationTree _animationTree;
	[Export] protected BehaviorTreeComponent _behaviorTree;

	public Node3D _dtc;

	public override void _Ready()
	{
		base._Ready();

		Brain.NavigationAgent.DebugEnabled = true;

		System.Diagnostics.Debug.Assert(Brain != null, "no brain component");
		System.Diagnostics.Debug.Assert(Brain.NavigationAgent != null, "No navigation agent");
		System.Diagnostics.Debug.Assert(MovementComponent != null, "No movement component");

		Brain.NavigationAgent.VelocityComputed += SetVelocity;

		CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Dev.UI.PushDevNotification("AI character health changed: " + newHealth);
		};

		CharacterMovement.EnableClimbing = false;
		CharacterMovement.EnableJumping = false;
		CharacterMovement.EnableSneaking = false;

		Brain.NavigationAgent.AvoidanceEnabled = false;

		_dtc = GetNodeOrNull<Node3D>("%dtc");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!_behaviorTree.Started && !Engine.IsEditorHint())
		{
			_behaviorTree.Start();
		}

		Vector3 direction = new();
		Vector3 lookTarget = new();
		Vector3 targetMovementPosition = new();

		// this should be moved to the brain component rather than the character somehow
		// if (Brain.GetCurrentFocus() != null && Brain.GetCurrentFocus() is Node3D n)
		// {
		// 	direction = (n.GlobalPosition - GlobalPosition).Normalized();
		// 	targetMovementPosition = n.GlobalPosition;
		// 	lookTarget = n.GlobalPosition;
		// }
		// else if (Brain.GetTargetPosition() != Vector3.Zero)
		// {
		// 	direction = (Brain.GetTargetPosition() - GlobalPosition).Normalized();
		// 	targetMovementPosition = Brain.GetTargetPosition();
		// 	lookTarget = Brain.GetTargetPosition();
		//
		// 	if (Brain.GetTargetPosition() != Brain.NavigationAgent.TargetPosition)
		// 	{
		// 		Brain.NavigationAgent.TargetPosition = Brain.GetTargetPosition();
		// 	}
		// }
		// else
		// {
		// 	GD.Print("nope");
		// }
		
		// for (var i = 0; i < GetSlideCollisionCount(); i++)
		// {
		// 	KinematicCollision3D collision3D = GetSlideCollision(i);
		// 	if (collision3D.GetCollider() is not RigidBody3D r || !IsOnFloor()) continue;
		// 	
		// 	var directionToCollision = (r.GlobalPosition - CharacterBody.GlobalPosition).Normalized();
		// 	var angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
		// 	var angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
		// 	angleFactor = Mathf.Round(angleFactor * 100) / 100;
		//
		// 	if (!(angleFactor > 0.5)) continue;
		// 	
		// 	r.ApplyCentralImpulse(-collision3D.GetNormal() * 4.0f * angleFactor);
		// 	r.ApplyImpulse(-collision3D.GetNormal() * 0.01f * angleFactor, collision3D.GetPosition());
		// }

		// float dist = (targetMovementPosition - GlobalPosition).Length();
		// float brakingDist = 2.0f;
		// float mult = Mathf.Clamp((dist / brakingDist), 0.0f, 1.0f);

		var mult = 1.0f;

		Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, GetWorld3D().DirectSpaceState) * mult;
		_animationTree.Set("parameters/test/blend_position", Velocity.Length());
		
		var t = GlobalTransform;
		t = t.InterpolateWith(t.LookingAt(lookTarget, Vector3.Up), 0.05f);
		GlobalTransform = t;

		var gr = GlobalRotation;
		gr.X = 0;
		gr.Z = 0;
		GlobalRotation = gr;
		MoveAndSlide();
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		MoveAndSlide();
	}
}
