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

		// MovementComponent.MovementStarted += () =>
		// {
		// 	MovementComponent.ToggleSprint();
		// };
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
				LookAt(n.GlobalPosition, Vector3.Up);
			}

			if (Brain.GetTargetPosition() != default)
			{
				// Vector3 pos = Global.Data.Player.GlobalPosition;
				// Vector3 dir = (pos - GlobalPosition).Normalized();
				// // LookAt(Global.Data.Player.GlobalPosition, Vector3.Up);

				// float DesiredRotationY = Mathf.Atan2(-dir.X, -dir.Z);

				// Vector3 targetRotation = GlobalRotation;
				// targetRotation.Y = Mathf.LerpAngle(GlobalRotation.Y, DesiredRotationY, (float)delta * 3);

				// GlobalRotation = targetRotation;

				Vector3 globalLookVector = (Brain.NavigationAgent.GetNextPathPosition() - GlobalPosition).Normalized();
				Vector2 controlVector = new Vector2(-globalLookVector.X, -globalLookVector.Z).Normalized();

				// Vector3 actualRotation = ForwardVector.Normalized();

				// DebugDraw.Line(Chest.GlobalPosition, Chest.GlobalPosition + new Vector3(-controlVector.X, 0f, -controlVector.Y) * 2f, Colors.Green);
				// DebugDraw.Line(Chest.GlobalPosition, Chest.GlobalPosition + new Vector3(-actualRotation.X, 0f, actualRotation.Z) * 1.5f, Colors.Yellow);

				// Vector2 controlVector2D = new Vector2(-controlVector.X, -controlVector.Y);
				// Vector2 actualVector2D = new Vector2(actualRotation.X, actualRotation.Z);
				// float angleRadians = controlVector2D.AngleTo(actualVector2D);
				// float angleDegrees = Mathf.RadToDeg(angleRadians);

				// GD.Print(-angleDegrees);

				Vector3 v = MovementComponent.CalculateMovementVelocity(controlVector, delta);
				// Brain.NavigationAgent.Velocity = v;
				Velocity = v;
				MoveAndSlide();

				// _animationTree.Set("parameters/TestRename/blend_position", new Vector2(-angleDegrees, 0f));
				// _animationTree.Set("parameters/Transition/transition_request", MovementComponent.IsMoving ? "moving" : "idle");

				if (_animationTree != null)
				{
					// GD.Print(Mathf.RadToDeg(y.AngleTo(x)));

					// _animationTree.Set("parameters/direction", angleDegrees);

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

		ForwardVector = -GlobalTransform.Basis.Z;
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		MoveAndSlide();
	}
}
