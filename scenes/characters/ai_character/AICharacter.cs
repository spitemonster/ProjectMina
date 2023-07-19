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

	public override void Attack()
	{
		base.Attack();
		_animPlayer.Play("Attack");
	}

	public override void _Ready()
	{
		base._Ready();

		Debug.Assert(_brainComponent != null, "no brain component");
		Debug.Assert(_navigationAgent != null, "No navigation agent");
		Debug.Assert(TempWeapon != null, "No temp weapon");
		Debug.Assert(_movementComponent != null, "No movement component");

		TempWeapon?.Equip(this);
		_navigationAgent.VelocityComputed += SetVelocity;

		CharacterHealthComponent.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Dev.UI.PushDevNotification("AI character health changed: " + newHealth);
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (_brainComponent.CurrentFocus != null)
		{
			GlobalTransform = GlobalTransform.InterpolateWith(GlobalTransform.LookingAt(_brainComponent.CurrentFocus.GlobalPosition, Vector3.Up), 8.0f * (float)delta);

			Vector3 currentRotation = GlobalRotation;
			currentRotation.X = 0;
			currentRotation.Z = 0;
			GlobalRotation = currentRotation;

			Vector3 globalLookVector = _brainComponent.CurrentFocus.GlobalPosition - GlobalPosition;
			Vector3 localLookVector = GlobalTransform.Basis.Inverse() * globalLookVector;
			Vector2 testVector = new Vector2(localLookVector.X, -localLookVector.Z).Normalized();
			_navigationAgent.Velocity = _movementComponent.CalculateMovementVelocity(testVector);
		}
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		float dist = (_navigationAgent.TargetPosition - GlobalPosition).Length() - 1.0f;
		float brakingDist = .5f;
		float mult = Mathf.Clamp(dist / brakingDist, 0.0f, 1.0f);

		mult = mult < .3f ? 0.0f : mult;

		Velocity = safeVelocity * mult;
		MoveAndSlide();
	}
}
