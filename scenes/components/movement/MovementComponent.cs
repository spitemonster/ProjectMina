using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class MovementComponent : ComponentBase
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

	[Signal] public delegate void MovementStartedEventHandler();
	[Signal] public delegate void MovementEndedEventHandler();
	[Signal] public delegate void SprintDesiredEventHandler(bool Sprinted);
	[Signal] public delegate void SprintStartedEventHandler();
	[Signal] public delegate void SprintEndedEventHandler();
	[Signal] public delegate void SneakDesiredEventHandler(bool Sneaked);
	[Signal] public delegate void SneakStartedEventHandler();
	[Signal] public delegate void SneakEndedEventHandler();
	[Signal] public delegate void JumpDesiredEventHandler(bool Jumped);
	[Signal] public delegate void JumpedEventHandler();
	[Signal] public delegate void FellEventHandler();
	[Signal] public delegate void LandedEventHandler(float fallDuration, Vector3 position);
	[Signal] public delegate void MovementStateChangedEventHandler(MovementState newState);

	public MovementState CharacterMovementState { get; protected set; }

	public Vector2 DesiredMovementDirection { get; private set; }

	[Export] protected float MovementSpeedBase = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;
	[Export] protected float BrakingForce = 0.1f;
	[Export] protected float JumpForce = 1.0f;
	[Export] protected float GravityMultiplier = 1.0f;
	[Export] protected float SprintSpeedMultiplier = 2.0f;
	[Export] protected float SneakSpeedMultiplier = 0.5f;
	[Export(PropertyHint.Range, "0,1,0.05")] protected float AirControlMultiplier = 0.5f;

	public bool Moving { get => _moving; }

	private float _gravity = -9.8f;
	private CharacterBase _character;
	private Timer _fallTimer;
	private bool _falling = false;
	private double _currentFallDuration = 0.0;
	private bool _sprinting = false;
	private bool _sneaking = false;
	private bool _moving = false;
	private bool _sprintDesired = false;
	private bool _sneakDesired = false;
	private bool _standDesired = false;
	private bool _leaning = false;
	private Enums.DirectionHorizontal _leanDirection;

	public Vector3 GetCharacterVelocity(Vector3 movementDirection, double delta)
	{

		Vector3 currentVelocity = _character.Velocity;

		float movementSpeed = CalculateMovementSpeed();

		// actual movement code
		if (!_character.IsOnFloor())
		{
			currentVelocity.Y -= (float)(_gravity * GravityMultiplier * delta);

			currentVelocity.X += (float)(movementDirection.X * AirControlMultiplier * delta);
			currentVelocity.Z += (float)(movementDirection.Z * AirControlMultiplier * delta);

			// only trigger falling when our velocity is actually negative
			if (currentVelocity.Y < 0.0f && !_falling)
			{
				StartFall();
			}

		}
		else
		{
			if (_falling)
			{
				Land();
			}

			if (movementDirection.Length() > 0.05)
			{
				currentVelocity.X = Mathf.MoveToward(currentVelocity.X, movementDirection.X * movementSpeed, AccelerationForce);
				currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, movementDirection.Z * movementSpeed, AccelerationForce);
			}
			else
			{
				currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0.0f, BrakingForce);
				currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0.0f, BrakingForce);
			}
		}

		// signal code
		if (currentVelocity.Length() >= 0.15 && !_moving)
		{
			_moving = true;
			EmitSignal(SignalName.MovementStarted);
		}
		else if (currentVelocity.Length() < 0.15 && _moving)
		{
			_moving = false;
			_sprinting = false;
			EmitSignal(SignalName.MovementEnded);
			SetMovementState(MovementState.NONE);
		}

		if (_moving && !_sprinting && CharacterMovementState != MovementState.WALKING)
		{
			SetMovementState(MovementState.WALKING);
		}

		if (_debug)
		{
			DebugDraw.Arrow(_character.GlobalPosition, currentVelocity.Normalized(), 1, Colors.Green);
		}

		return currentVelocity;
	}

	public override void _Ready()
	{
		base._Ready();

		if (_debug)
		{
			System.Diagnostics.Debug.Assert(GetOwner<CharacterBase>() is CharacterBase, "Movement Component should only belong to a CharacterBase.");
		}

		_character = GetOwner<CharacterBase>();

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

	/// <summary>
	/// 	Get a character's velocity 
	/// </summary>

	/// <param name="delta"></param>
	/// <returns>
	/// 	Vector3 representing the character's Velocity after movement speed and external factors are applied.
	/// </returns>
	public override void _PhysicsProcess(double delta)
	{
		if (_character is not CharacterBase && _debug)
		{
			GD.PushError("MovementComponent should only be a child of a class inheriting CharacterBase");
			return;
		}

		base._PhysicsProcess(delta);

	}

	public void Lean(uint direction, bool end)
	{
		Enums.DirectionHorizontal dir = (Enums.DirectionHorizontal)direction;
		if (dir == 0 || (_leaning && dir != _leanDirection))
		{
			if (_debug)
			{
				GD.PushError("should not lean");
			}
			return;
		}
		else
		{
			switch (dir)
			{
				case Enums.DirectionHorizontal.Right:
					if (end)
					{
						_character.AnimPlayer.PlayBackwards("lean_right");
						_leaning = false;
						_leanDirection = Enums.DirectionHorizontal.None;
					}
					else
					{
						_character.AnimPlayer.Play("lean_right");
						_leaning = true;
						_leanDirection = dir;
					}
					break;
				default:
					if (end)
					{
						_character.AnimPlayer.PlayBackwards("lean_left");
						_leaning = false;
						_leanDirection = Enums.DirectionHorizontal.None;
					}
					else
					{
						_character.AnimPlayer.Play("lean_left");
						_leaning = true;
						_leanDirection = dir;
					}
					break;
			}
		}
	}

	public float CalculateMovementSpeed()
	{
		float localMovementSpeed = MovementSpeedBase;

		if (_sprinting)
		{
			localMovementSpeed *= SprintSpeedMultiplier;
		}
		else if (_sneaking)
		{
			localMovementSpeed *= SneakSpeedMultiplier;
		}

		return localMovementSpeed;
	}

	public void Jump()
	{
		Vector3 currentVelocity = _character.Velocity;
		currentVelocity.Y = JumpForce;
		_character.Velocity = currentVelocity;
		SetMovementState(MovementState.JUMPING);
	}

	public void ToggleSprint()
	{
		if (_sprinting)
		{
			EndSprint();
		}
		else if (_moving)
		{
			StartSprint();
		}
	}

	public void ToggleSneak()
	{
		if (_sneaking)
		{
			EndSneak();
		}
		else
		{
			if (_sprinting)
			{
				EndSprint();
			}

			StartSneak();
		}
	}

	// private void TryStand()
	// {
	// 	if (CanStand())
	// 	{
	// 		Stand();
	// 	}
	// 	else
	// 	{
	// 		_wishStand = true;
	// 	}
	// }

	// private void TrySneak()
	// {
	// 	if (CanSneak())
	// 	{
	// 		Sneak();
	// 	}
	// 	else
	// 	{
	// 		_wishSneak = true;
	// 	}
	// }

	// private bool CanStand()
	// {
	// 	return true;
	// }

	// private void Stand()
	// {
	// 	// character stands to full height here
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);

	// 	if (_owner == null)
	// 	{
	// 		return;
	// 	}

	// 	if (_moving && _owner.Velocity.Length() < 0.025)
	// 	{
	// 		_moving = false;
	// 		_sprinting = false;
	// 		EmitSignal(SignalName.MovementEnded);
	// 		SetMovementState(MovementState.NONE);
	// 	}
	// 	else if (!_moving && _owner.Velocity.Length() > 0.1)
	// 	{
	// 		_moving = true;
	// 		EmitSignal(SignalName.MovementStarted);
	// 		SetMovementState(MovementState.WALKING);
	// 	}

	// 	// check if owner is on the floor and the character isn't on the upswing
	// 	if (!_owner.IsOnFloor() && !_falling)
	// 	{
	// 		// if not, run fall logic
	// 		HandleFall();
	// 	}
	// 	else if (_owner.IsOnFloor() && _falling)
	// 	{
	// 		// if we were falling but have landed, trigger landed
	// 		HandleLanding();
	// 	}
	// }

	// private float CalculateMovementSpeed()
	// {
	// 	float localMovementSpeed = MovementSpeed;

	// 	if (_sprinting)
	// 	{
	// 		localMovementSpeed *= SprintMultiplier;
	// 	}
	// 	else if (_sneaking)
	// 	{
	// 		localMovementSpeed *= SneakMultiplier;
	// 	}

	// 	return localMovementSpeed;
	// }

	private void SetMovementState(MovementState newState)
	{
		if (newState == CharacterMovementState)
		{
			return;
		}

		CharacterMovementState = newState;

		EmitSignal(SignalName.MovementStateChanged, (int)newState);
	}

	private void StartSprint()
	{
		_sprinting = true;
		EmitSignal(SignalName.SprintStarted);
		SetMovementState(MovementState.SPRINTING);
	}

	private void EndSprint()
	{
		_sprinting = false;
		EmitSignal(SignalName.SprintEnded);
	}

	private void StartSneak()
	{
		_sneaking = true;
		EmitSignal(SignalName.SneakStarted);
		SetMovementState(MovementState.SNEAKING);
	}

	private void EndSneak()
	{
		_sneaking = false;
		EmitSignal(SignalName.SneakEnded);
	}

	private void StartFall()
	{
		if (!_falling)
		{
			_falling = true;
			_fallTimer.Start();
			EmitSignal(SignalName.Fell);
			SetMovementState(MovementState.FALLING);
		}
	}

	private void Land()
	{
		if (_falling)
		{
			_falling = false;
			_fallTimer.Stop();
			EmitSignal(SignalName.Landed, _currentFallDuration, _character.GlobalPosition);
			_currentFallDuration = 0.0;
			SetMovementState(MovementState.NONE);
		}
	}
}
