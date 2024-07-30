using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class MovementComponent : ComponentBase
{
	public enum EMovementState: int
	{
		None,
		Walking,
		Sprinting,
		Falling,
		Jumping,
		Climbing,
		Sneaking
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
	[Signal] public delegate void MovementStateChangedEventHandler(EMovementState newState);
	[Export] public bool EnableClimbing = true;
	[Export] public bool EnableJumping = true;
	[Export] public bool EnableSneaking = true;
	public EMovementState CharacterMovementState { get; protected set; }
	public Vector2 DesiredMovementDirection { get; private set; }
	[Export] protected float MovementSpeedBase = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;
	[Export] protected float BrakingForce = 0.1f;
	[Export] protected float JumpForce = 1.0f;
	[Export] protected float ClimbSpeed = 1.5f;
	[Export] protected float GravityMultiplier = 1.0f;
	[Export] protected float SprintSpeedMultiplier = 2.0f;
	[Export] protected float SneakSpeedMultiplier = 0.5f;
	[Export(PropertyHint.Range, "0,1,0.05")] protected float AirControlMultiplier = 0.5f;

	public bool IsSneaking => _sneaking;

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
	private bool _wishJump = false;
	private bool _wishClimb = false;
	private bool _wishStand = false;
	private Vector3 _climbPosition = new();

	Array<Rid> _exclude = new();
	PhysicsPointQueryParameters3D _climbPointQueryParams = new();

	private Enums.DirectionHorizontal _leanDirection;

	public Vector3 GetCharacterVelocity(Vector3 movementDirection, double delta, PhysicsDirectSpaceState3D spaceState, PhysicsMaterial currentFloor = null)
	{
		float frictionMultiplier = 1.0f;

		if (currentFloor != null)
		{
			frictionMultiplier = currentFloor.Friction;
		}

		frictionMultiplier = Mathf.Clamp(frictionMultiplier, 0.015f, 1.0f);

		Vector3 currentVelocity = _character.Velocity;
		float movementSpeed = CalculateMovementSpeed();

		if (_wishClimb && CanClimb(spaceState) && CharacterMovementState != EMovementState.Climbing)
		{
			StartClimb(spaceState);
		}
		else if (_wishJump && CanJump(spaceState) && CharacterMovementState != EMovementState.Climbing)
		{
			currentVelocity = Jump(currentVelocity);
		}

		if (_wishStand && CanStand(spaceState))
		{
			EndSneak();
		}

		if (CharacterMovementState == EMovementState.Climbing)
		{
			currentVelocity = TickClimb(spaceState);
		}
		else if (!_character.IsOnFloor())
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
				currentVelocity.X = Mathf.MoveToward(currentVelocity.X, movementDirection.X * movementSpeed, AccelerationForce * frictionMultiplier);
				currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, movementDirection.Z * movementSpeed, AccelerationForce * frictionMultiplier);
			}
			else
			{
				currentVelocity.X = Mathf.MoveToward(currentVelocity.X, 0.0f, BrakingForce * frictionMultiplier);
				currentVelocity.Z = Mathf.MoveToward(currentVelocity.Z, 0.0f, BrakingForce * frictionMultiplier);
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
			SetMovementState(EMovementState.None);
		}

		if (_moving && !_sprinting && CharacterMovementState != EMovementState.Walking && CharacterMovementState != EMovementState.Climbing)
		{
			SetMovementState(EMovementState.Walking);
		}

		if (EnableDebug)
		{
			DebugDraw.Arrow(_character.GlobalPosition, currentVelocity.Normalized(), 1, Colors.Green);
		}

		return currentVelocity;
	}

	public override void _Ready()
	{
		base._Ready();

		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(GetOwner<CharacterBase>() != null, "Movement Component should only belong to a CharacterBase.");
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

		_exclude.Add(_character.GetRid());
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
		if (_character is not CharacterBase && EnableDebug)
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
			if (EnableDebug)
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

	public void TryJump()
	{
		if (_character.IsOnFloor())
		{
			_wishJump = true;
		}
		
		_wishClimb = true;
	}

	private bool CanJump(PhysicsDirectSpaceState3D spaceState)
	{
		if (_character.IsOnFloor() && CharacterMovementState != EMovementState.Sneaking)
		{
			return true;
		}

		return false;
	}

	private Vector3 Jump(Vector3 currentVelocity)
	{
		Vector3 newVelocity = currentVelocity;
		newVelocity.Y = JumpForce;
		_wishJump = false;
		_wishClimb = false;
		SetMovementState(EMovementState.Jumping);
		return newVelocity;
	}

	public void JumpReleased()
	{
		_wishClimb = false;

		if (CharacterMovementState == EMovementState.Climbing)
		{
			EndClimb();
		}
	}

	private void StartClimb(PhysicsDirectSpaceState3D spaceState)
	{
		Vector3 downTraceOrigin = _character.GlobalPosition + (_character.ForwardVector * .75f) + new Vector3(0, 2, 0);
		Vector3 downTraceEnd = downTraceOrigin + new Vector3(0, -.7f, 0);

		HitResult res = Cast.Ray(spaceState, downTraceOrigin, downTraceEnd, _exclude, true, true);

		if (res != null)
		{
			EndSprint();
			_wishClimb = false;
			_climbPosition = res.HitPosition;
			SetMovementState(EMovementState.Climbing);
		}
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
			_wishStand = true;
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

	private void SetMovementState(EMovementState newState)
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
		SetMovementState(EMovementState.Sprinting);
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
		SetMovementState(EMovementState.Sneaking);
	}

	private void EndSneak()
	{
		_sneaking = false;
		_wishStand = false;
		EmitSignal(SignalName.SneakEnded);
	}

	private void StartFall()
	{
		if (!_falling)
		{
			_falling = true;
			_fallTimer.Start();
			EmitSignal(SignalName.Fell);
			SetMovementState(EMovementState.Falling);
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
			SetMovementState(EMovementState.None);
		}
	}

	private bool CanClimb(PhysicsDirectSpaceState3D spaceState)
	{
		if (spaceState == null || CharacterMovementState == EMovementState.Sneaking || !EnableClimbing)
		{
			return false;
		}

		var downTraceOrigin = _character.GlobalPosition + (_character.ForwardVector * .75f) + new Vector3(0, 2, 0);
		_climbPointQueryParams.Position = downTraceOrigin;
		_climbPointQueryParams.Exclude = _exclude;

		var tres = spaceState.IntersectPoint(_climbPointQueryParams);
		
		if (tres.Count > 0)
		{
			return false;
		}

		var downTraceEnd = downTraceOrigin + new Vector3(0, -1, 0);
		
		DebugDraw.Sphere(downTraceOrigin, .5f, Colors.Red);
		DebugDraw.Sphere(downTraceEnd, .4f, Colors.Green);

		var res = Cast.Ray(spaceState, downTraceOrigin, downTraceEnd, _exclude, true, true);

		if (res == null) return false;
		
		DebugDraw.Sphere(res.HitPosition, .6f, Colors.Yellow, .5f);
		return true;
	}


	private Vector3 TickClimb(PhysicsDirectSpaceState3D spaceState)
	{

		var forwardTraceOrigin = _character.GlobalPosition;
		var forwardTraceEnd = forwardTraceOrigin + _character.ForwardVector * -1.0f;
		var forwardTraceRes = Cast.Ray(spaceState, forwardTraceOrigin, forwardTraceEnd, _exclude, true, true);

		if (_character.GlobalPosition.Y >= _climbPosition.Y)
		{
			var directionToClimbPosition = (_climbPosition - _character.GlobalPosition).Normalized();

			var downTraceOrigin = forwardTraceOrigin + directionToClimbPosition * -.25f;
			var downTraceEnd = downTraceOrigin + new Vector3(0, -1, 0);

			var downTraceRes = Cast.Ray(spaceState, downTraceOrigin, downTraceEnd, _exclude, true, true);

			if (downTraceRes == null) return directionToClimbPosition * ClimbSpeed;
			
			_climbPosition = new Vector3();
			EndClimb();
			return new Vector3();


		}
		else
		{
			return new Vector3(0, ClimbSpeed, 0);
		}
	}

	private void EndClimb()
	{
		_wishClimb = false;
		_wishJump = false;
		SetMovementState(EMovementState.None);
	}

	private bool CanStand(PhysicsDirectSpaceState3D spaceState)
	{
		var standTraceOrigin = _character.GlobalPosition + new Vector3(0, .5f, 0);
		var standTraceEnd = standTraceOrigin + new Vector3(0, 1, 0);
		var standTraceResult = Cast.Ray(spaceState, standTraceOrigin, standTraceEnd, _exclude, true, true);

		return standTraceResult == null;
	}

}
