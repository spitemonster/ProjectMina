using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class MovementComponent : Node
{
	public enum MovementState
	{
		NONE,
		WALKING,
		SPRINTING,
		FALLING,
		JUMPING,
		SNEAKING
	}

	[Signal]
	public delegate void MovementStartedEventHandler();
	[Signal]
	public delegate void MovementEndedEventHandler();
	[Signal]
	public delegate void SprintStartedEventHandler();
	[Signal]
	public delegate void SprintEndedEventHandler();
	[Signal]
	public delegate void SneakStartedEventHandler();
	[Signal]
	public delegate void SneakEndedEventHandler();
	[Signal]
	public delegate void FellEventHandler();
	[Signal]
	public delegate void LandedEventHandler(float fallDuration, Vector3 position);
	[Signal] public delegate void MovementStateChangedEventHandler(MovementState newState);
	public bool IsSprinting { get => _sprinting; }
	public bool IsMoving { get => _moving; }

	public MovementState CharacterMovementState;

	public Vector2 DesiredMovementDirection { get; private set; }

	[Export]
	protected float MovementSpeed = 1.0f;
	[Export]
	protected float AccelerationForce = 1.0f;
	[Export]
	protected float BrakingForce = 1.0f;
	[Export]
	protected float JumpForce = 1.0f;
	[Export]
	protected float GravityMultiplier = 1.0f;
	[Export]
	protected float SprintMultiplier = 2.0f;
	[Export]
	protected float SneakMultiplier = 0.5f;

	private float _gravity = 9.8f;
	private CharacterBase _owner;
	private Timer _fallTimer;
	private bool _falling = false;
	private double _currentFallDuration = 0.0;
	private bool _sprinting = false;
	private bool _sneaking = false;
	private bool _moving = false;

	public Vector3 CalculateMovementVelocity(Vector2 inputVector, double delta)
	{

		Vector3 direction = _owner.GlobalTransform.Basis * new Vector3(-inputVector.X, 0, -inputVector.Y);
		Vector3 currentVelocity = _owner.Velocity;
		float movementSpeed = CalculateMovementSpeed();

		if (inputVector.Length() > 0.1)
		{
			currentVelocity.X = Mathf.MoveToward(currentVelocity.X, direction.X * movementSpeed, AccelerationForce);
			currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, direction.Z * movementSpeed, AccelerationForce);
		}
		else
		{
			currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0.0f, BrakingForce);
			currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0.0f, BrakingForce);
		}

		if (!_owner.IsOnFloor())
		{
			currentVelocity.Y -= (float)(_gravity * GravityMultiplier * delta);
		}

		return currentVelocity;
	}

	public void Jump()
	{
		Vector3 currentVelocity = _owner.Velocity;
		currentVelocity.Y = JumpForce;
		_owner.Velocity = currentVelocity;
		SetMovementState(MovementState.JUMPING);
	}

	public void ToggleSprint()
	{
		_sprinting = !_sprinting;

		if (_sprinting)
		{
			SetMovementState(MovementState.SPRINTING);
			_sneaking = false;
		}
	}

	public void ToggleSneak()
	{
		_sneaking = !_sneaking;

		if (_sneaking)
		{
			SetMovementState(MovementState.SNEAKING);
			_sprinting = false;
		}
	}

	public override void _Ready()
	{
		_owner = GetOwner<CharacterBase>();
		// get gravity project settings
		_gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		_fallTimer = new()
		{
			WaitTime = .1,
			Autostart = false,
			OneShot = false
		};
		AddChild(_fallTimer);

		_fallTimer.Timeout += () =>
		{
			_currentFallDuration += .1;
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_owner == null)
		{
			return;
		}

		if (_moving && _owner.Velocity.Length() < 0.025)
		{
			_moving = false;
			_sprinting = false;
			EmitSignal(SignalName.MovementEnded);
			SetMovementState(MovementState.NONE);
		}
		else if (!_moving && _owner.Velocity.Length() > 0.1)
		{
			_moving = true;
			EmitSignal(SignalName.MovementStarted);
			SetMovementState(MovementState.WALKING);
		}

		// check if owner is on the floor and the character isn't on the upswing
		if (!_owner.IsOnFloor() && _owner.Velocity.Y < 0)
		{
			// if not, run fall logic
			HandleFall();
			Vector3 currentVelocity = _owner.Velocity;
			currentVelocity.Y -= (float)(_gravity * GravityMultiplier * delta);
		}
		else
		{
			// if we were falling but have landed, trigger landed
			HandleLanding();
		}
	}

	private float CalculateMovementSpeed()
	{
		float localMovementSpeed = MovementSpeed;

		if (_sprinting)
		{
			localMovementSpeed *= SprintMultiplier;
		}
		else if (_sneaking)
		{
			localMovementSpeed *= SneakMultiplier;
		}

		return localMovementSpeed;
	}

	private void HandleFall()
	{
		if (!_falling)
		{
			_falling = true;
			_fallTimer.Start();
			EmitSignal(SignalName.Fell);
			SetMovementState(MovementState.FALLING);
		}
	}

	private void HandleLanding()
	{
		if (_falling)
		{
			_falling = false;
			_fallTimer.Stop();
			EmitSignal(SignalName.Landed, _currentFallDuration, _owner.GlobalPosition);
			_currentFallDuration = 0.0;
			SetMovementState(MovementState.NONE);
		}
	}

	private void SetMovementState(MovementState newState)
	{
		if (newState == CharacterMovementState)
		{
			return;
		}

		CharacterMovementState = newState;

		EmitSignal(SignalName.MovementStateChanged, (int)newState);
	}
}
