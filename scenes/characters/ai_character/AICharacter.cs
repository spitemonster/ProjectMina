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
	[Export] public NavigationAgent3D NavigationAgent { get; protected set; }
	[Export] public MovementComponent MovementComponent { get; protected set; }

	[Export] protected AnimationPlayer _animPlayer;
	[Export] protected MeleeWeapon TempWeapon;
	[Export] protected AIBrainComponent _brainComponent;
	[Export] protected AnimationTree _animationTree;
	[Export] protected BehaviorTreeComponent _behaviorTree;

	public override void Attack()
	{
		base.Attack();
	}

	public override void _Ready()
	{
		base._Ready();

		NavigationAgent.DebugEnabled = true;

		Debug.Assert(_brainComponent != null, "no brain component");
		Debug.Assert(NavigationAgent != null, "No navigation agent");
		Debug.Assert(MovementComponent != null, "No movement component");

		NavigationAgent.VelocityComputed += SetVelocity;

		CharacterHealthComponent.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Dev.UI.PushDevNotification("AI character health changed: " + newHealth);
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (IsOnFloor())
		{
			if (!_behaviorTree.Started)
			{
				_behaviorTree.Start();
			}

			// THIS HAS TO BE HERE YA FUCKIN IDJIT
			if (!NavigationAgent.AvoidanceEnabled)
			{
				NavigationAgent.AvoidanceEnabled = true;
			}

			if (NavigationAgent.TargetPosition != new Vector3(0.0f, 0.0f, 0.0f))
			{
				GlobalTransform = GlobalTransform.InterpolateWith(GlobalTransform.LookingAt(NavigationAgent.GetNextPathPosition(), Vector3.Up), 8.0f * (float)delta);

				Vector3 currentRotation = GlobalRotation;
				currentRotation.X = 0;
				currentRotation.Z = 0;
				GlobalRotation = currentRotation;

				Vector3 globalLookVector = NavigationAgent.GetNextPathPosition() - GlobalPosition;
				Vector3 localLookVector = GlobalTransform.Basis.Inverse() * globalLookVector;
				Vector2 testVector = new Vector2(localLookVector.X, -localLookVector.Z).Normalized();
				Vector3 v = MovementComponent.CalculateMovementVelocity(testVector, delta);

				NavigationAgent.Velocity = v;

				if (_animationTree != null)
				{
					_animationTree.Set("parameters/locomotion/blend_position", MovementComponent.IsSprinting ? 1.0f : 0.0f);
					_animationTree.Set("parameters/Transition/transition_request", MovementComponent.IsMoving ? "moving" : "idle");
				}
			}



		}
		else
		{
			if (NavigationAgent.AvoidanceEnabled)
			{
				NavigationAgent.AvoidanceEnabled = false;
			}

			Velocity = MovementComponent.CalculateMovementVelocity(Vector2.Zero, delta);
			MoveAndSlide();
		}
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		MoveAndSlide();
	}
}
