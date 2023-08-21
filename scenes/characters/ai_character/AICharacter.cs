using Godot;
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
	[Export]
	protected AnimationPlayer _animPlayer;
	[Export]
	protected MeleeWeapon TempWeapon;
	[Export]
	protected MovementComponent _movementComponent;
	[Export]
	protected AIBrainComponent _brainComponent;
	[Export]
	protected NavigationAgent3D _navigationAgent;
	[Export]
	protected AnimationTree _animationTree;

	public override void Attack()
	{
		base.Attack();
		// _animPlayer.Play("Attack");
	}

	public override void _Ready()
	{
		base._Ready();

		Debug.Assert(_brainComponent != null, "no brain component");
		Debug.Assert(_navigationAgent != null, "No navigation agent");
		Debug.Assert(_movementComponent != null, "No movement component");

		_navigationAgent.VelocityComputed += SetVelocity;

		CharacterHealthComponent.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Dev.UI.PushDevNotification("AI character health changed: " + newHealth);
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		Vector3 LookVector;

		if (IsOnFloor())
		{
			if (!_navigationAgent.AvoidanceEnabled)
			{
				_navigationAgent.AvoidanceEnabled = true;
			}

			if (_brainComponent.CurrentFocus != null)
			{
				_navigationAgent.TargetPosition = _brainComponent.CurrentFocus.GlobalPosition;
				GlobalTransform = GlobalTransform.InterpolateWith(GlobalTransform.LookingAt(_brainComponent.CurrentFocus.GlobalPosition, Vector3.Up), 8.0f * (float)delta);

				Vector3 currentRotation = GlobalRotation;
				currentRotation.X = 0;
				currentRotation.Z = 0;
				GlobalRotation = currentRotation;

				Vector3 globalLookVector = _navigationAgent.GetNextPathPosition() - GlobalPosition;
				Vector3 localLookVector = GlobalTransform.Basis.Inverse() * globalLookVector;
				Vector2 testVector = new Vector2(localLookVector.X, -localLookVector.Z).Normalized();
				Vector3 v = _movementComponent.CalculateMovementVelocity(testVector, delta);

				_navigationAgent.Velocity = v;

				if (_animationTree != null)
				{

					_animationTree.Set("parameters/locomotion/blend_position", _movementComponent.IsSprinting ? 1.0f : 0.0f);
					_animationTree.Set("parameters/Transition/transition_request", _movementComponent.IsMoving ? "moving" : "idle");
				}
			}
			else
			{
				_animationTree.Set("parameters/locomotion/blend_position", 0.0f);
				_animationTree.Set("parameters/Transition/transition_request", "idle");
			}
		}
		else
		{
			if (_navigationAgent.AvoidanceEnabled)
			{
				_navigationAgent.AvoidanceEnabled = false;
			}
			Velocity = _movementComponent.CalculateMovementVelocity(Vector2.Zero, delta);
			MoveAndSlide();
		}
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		// float dist = (_navigationAgent.TargetPosition - GlobalPosition).Length() - 1.0f;
		// float brakingDist = .5f;
		// float mult = Mathf.Clamp(dist / brakingDist, 0.0f, 1.0f);

		// mult = mult < .3f ? 0.0f : mult;

		Velocity = safeVelocity;
		MoveAndSlide();
	}
}
