using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class PlayerCharacter : CharacterBase
{

	[ExportGroup("CharacterBase")]
	[Export]
	protected double _movementSpeed = 5.0;
	[Export]
	protected double _sprintMultiplier = 1.5;
	[Export]
	protected double _brakingForce = 0.3;
	[Export]
	protected double _gravityMultiplier = 1.0;

	[ExportGroup("PlayerCharacter")]
	[Export]
	public Camera3D PrimaryCamera { get; protected set; }
	[Export]
	protected InteractionComponent _interactionComponent;
	[Export]
	protected Node3D _head;
	[Export]
	protected double _jumpForce = 5.0;
	[Export]
	protected double _inputSensitivity = .0025;

	[ExportGroup("Grabbing")]
	[Export]
	protected double _carryGrabSpeedMultiplier = 1.0f;
	[Export]
	protected double _carryMovementSpeedMultiplier = 0.5f;

	public bool IsSprinting { get; protected set; }
	private InputManager _inputManager;
	private double _gravity;


	[ExportGroup("Stealth")]
	[Export]
	protected float _stealthCapsuleHeight;
	[Export]
	protected Vector3 _stealthCapsulePosition;


	private Node3D _currentFloor;
	private bool _isStealthMode = false;
	private float _defaultCapsuleHeight;
	private Vector3 _defaultCapsulePosition;

	public override void _Ready()
	{
		// get gravity project settings
		_gravity = (double)ProjectSettings.GetSetting("physics/3d/default_gravity");

		Global.Data.Player = this;

		// ensure there is an input manager before binding events to it
		if (GetNode("/root/InputManager") is InputManager m)
		{
			_inputManager = m;
			_inputManager.MouseMove += HandleMouseMove;
			_inputManager.Sprint += ToggleSprint;
			_inputManager.Jump += Jump;
			_inputManager.Stealth += ToggleStealth;
		}

		// get our default capsule settings for crouching
		CapsuleShape3D bodyCapsule = (CapsuleShape3D)CharacterBody.Shape;
		_defaultCapsuleHeight = bodyCapsule.Height;
		_defaultCapsulePosition = CharacterBody.Position;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 currentVelocity = Velocity;
		Vector2 InputDirection = InputManager.GetInputDirection();
		Vector3 Direction = _head.Transform.Basis * new Vector3(InputDirection.X, 0, InputDirection.Y);

		if (!IsOnFloor())
		{
			currentVelocity.Y -= (float)(_gravity * delta * _gravityMultiplier);
			if (_currentFloor is RigidBody3D r)
			{
				r.Sleeping = false;
			}
			_currentFloor = null;
		}
		else
		{
			// if the player is on the floor, update the _currentFloor value because we're using it down the line
			PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
			Godot.Collections.Array<Rid> x = new() { GetRid() };
			Godot.Collections.Dictionary traceResult = Trace.Line(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, x);

			traceResult.TryGetValue("collider", out Variant collider);
			if (collider.As<Node3D>() is Node3D n)
			{
				if (n is RigidBody3D r)
				{
					_currentFloor = r;
					r.Sleeping = true;
				}
			}
		}

		// iterate over all collisions and apply collision physics to them
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision3D = GetSlideCollision(i);
			if (collision3D.GetCollider() is RigidBody3D r && IsOnFloor() && r != _currentFloor)
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


		// character movement
		double targetMovementSpeed = _movementSpeed * (IsSprinting ? _sprintMultiplier : 1.0f);

		if (_interactionComponent.IsGrabbing)
		{
			targetMovementSpeed *= _carryMovementSpeedMultiplier;
		}

		if (InputDirection.Length() > 0.1f)
		{
			currentVelocity.X = (float)Mathf.MoveToward(currentVelocity.X, Direction.X * targetMovementSpeed, 0.1f);
			currentVelocity.Z = (float)Mathf.MoveToward(currentVelocity.Z, Direction.Z * targetMovementSpeed, 0.1f);
		}
		else
		{
			currentVelocity.X = (float)Mathf.MoveToward(currentVelocity.X, 0.0, _brakingForce);
			currentVelocity.Z = (float)Mathf.MoveToward(currentVelocity.Z, 0.0, _brakingForce);
		}

		Velocity = currentVelocity;

		_ = MoveAndSlide();
	}

	protected void Jump()
	{
		if (IsOnFloor())
		{
			Vector3 currentVelocity = Velocity;
			currentVelocity.Y += (float)_jumpForce;

			Velocity = currentVelocity;
		}
	}

	/*
	* Function: HandleMouseMove
	* TODO.
	*
	* Parameters:
	* mouseRelative - TODO.
	*
	* Returns:
	* (private void ) - the returned value.
	*
	*/
	private void HandleMouseMove(Vector2 mouseRelative)
	{
		if (!GetTree().Paused)
		{
			float headRelative = (float)(-mouseRelative.X * _inputSensitivity);
			float cameraRelative = (float)(-mouseRelative.Y * _inputSensitivity);

			if (_interactionComponent.IsGrabbing)
			{
				headRelative *= (float)_carryGrabSpeedMultiplier;
				cameraRelative *= (float)_carryGrabSpeedMultiplier;
			}

			_head.RotateY(headRelative);
			PrimaryCamera.RotateX(cameraRelative);

			Vector3 clampedCameraRotation = new()
			{
				X = Mathf.Clamp(PrimaryCamera.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80))
			};

			PrimaryCamera.Rotation = clampedCameraRotation;
		}
	}

	private void ToggleStealth()
	{
		_isStealthMode = !_isStealthMode;
		CapsuleShape3D capsule = (CapsuleShape3D)CharacterBody.Shape;

		if (_isStealthMode)
		{
			capsule.Height = (float)_stealthCapsuleHeight;
			CharacterBody.Position = _stealthCapsulePosition;
		}
		else
		{
			capsule.Height = (float)_defaultCapsuleHeight;
			CharacterBody.Position = _defaultCapsulePosition;
		}
	}

	private void ToggleSprint()
	{
		IsSprinting = !IsSprinting;
	}
}