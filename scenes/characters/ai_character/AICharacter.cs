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
	[Export] protected AnimationPlayer _animPlayer;
	[Export] protected MeleeWeapon TempWeapon;
	[Export] protected AnimationTree _animationTree;
	[Export] protected BehaviorTreeComponent _behaviorTree;

	public override void Attack()
	{
		base.Attack();
	}

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

		Brain.NavigationAgent.AvoidanceEnabled = false;
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

		if (Brain.GetCurrentFocus() != null && Brain.GetCurrentFocus() is Node3D n)
		{
			direction = (n.GlobalPosition - GlobalPosition).Normalized();
			targetMovementPosition = n.GlobalPosition;
			lookTarget = n.GlobalPosition;
		}
		else if (Brain.GetTargetPosition() != Vector3.Zero)
		{
			direction = (Brain.GetTargetPosition() - GlobalPosition).Normalized();
			targetMovementPosition = Brain.GetTargetPosition();
			lookTarget = Brain.GetTargetPosition();

			if (Brain.GetTargetPosition() != Brain.NavigationAgent.TargetPosition)
			{
				Brain.NavigationAgent.TargetPosition = Brain.GetTargetPosition();
			}
		}
		else
		{
			GD.Print("nope");
		}

		float dist = (targetMovementPosition - GlobalPosition).Length();
		float brakingDist = 2.0f;
		float mult = Mathf.Clamp((dist / brakingDist), 0.0f, 1.0f);

		mult = mult < .6f ? 0 : mult;

		Velocity = CharacterMovement.GetCharacterVelocity(direction, delta) * mult;

		Transform3D t = GlobalTransform;
		t = t.InterpolateWith(t.LookingAt(lookTarget, Vector3.Up), 0.05f);
		GlobalTransform = t;

		Vector3 gr = GlobalRotation;
		gr.X = 0;
		gr.Z = 0;
		GlobalRotation = gr;
		MoveAndSlide();

		_animationTree.Set("parameters/Transition/transition_request", MovementComponent.Moving ? "moving" : "idle");
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		MoveAndSlide();
	}
}
