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

		Debug.Assert(Brain != null, "no brain component");
		Debug.Assert(Brain.NavigationAgent != null, "No navigation agent");
		Debug.Assert(MovementComponent != null, "No movement component");

		Brain.NavigationAgent.VelocityComputed += SetVelocity;

		CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
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
			if (!Brain.NavigationAgent.AvoidanceEnabled)
			{
				Brain.NavigationAgent.AvoidanceEnabled = true;
			}

			if (Brain.GetCurrentFocus() is Node3D n)
			{
				GlobalTransform = GlobalTransform.InterpolateWith(GlobalTransform.LookingAt(n.GlobalPosition, Vector3.Up), 8.0f * (float)delta);

				Vector3 currentRotation = GlobalRotation;
				currentRotation.X = 0;
				currentRotation.Z = 0;
				GlobalRotation = currentRotation;
			}



			if (Brain.GetTargetPosition() != default)
			{

				GD.Print(Brain.GetTargetPosition());
				Vector3 globalLookVector = Brain.NavigationAgent.GetNextPathPosition() - GlobalPosition;
				Vector3 localLookVector = GlobalTransform.Basis.Inverse() * globalLookVector;
				Vector2 testVector = new Vector2(localLookVector.X, -localLookVector.Z).Normalized();


				Vector3 v = MovementComponent.CalculateMovementVelocity(testVector, delta);
				Brain.NavigationAgent.Velocity = v;

				if (_animationTree != null)
				{
					_animationTree.Set("parameters/locomotion/blend_position", MovementComponent.IsSprinting ? 1.0f : 0.0f);
					_animationTree.Set("parameters/Transition/transition_request", MovementComponent.IsMoving ? "moving" : "idle");
				}
			}

			// iterate over all collisions and apply collision physics to them
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				KinematicCollision3D collision3D = GetSlideCollision(i);
				if (collision3D.GetCollider() is RigidBody3D r && IsOnFloor())
				{
					Vector3 directionToCollision = (r.GlobalPosition - CharacterBody.GlobalPosition).Normalized();
					float angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
					float angleFactor = angleToCollision / (Mathf.Pi / 2);
					angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
					angleFactor = Mathf.Round(angleFactor * 100) / 100;

					if (angleFactor > 0.75)
					{
						r.ApplyCentralImpulse(-collision3D.GetNormal() * 20.0f * angleFactor);
						r.ApplyImpulse(-collision3D.GetNormal() * 2.0f * angleFactor, collision3D.GetPosition());
					}
				}
			}
		}
		else
		{
			if (Brain.NavigationAgent.AvoidanceEnabled)
			{
				Brain.NavigationAgent.AvoidanceEnabled = false;
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
