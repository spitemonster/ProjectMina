using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class PlayerCharacter : CharacterBase
{

	[Export]
	protected RangedWeapon gun;

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
	protected MovementComponent CharacterMovement { get; set; }

	[Export]
	protected InteractionComponent _interactionComponent;
	[Export]
	protected EquipmentManager _equipmentManager;

	[Export]
	protected Node3D _head;
	[Export]
	protected double _jumpForce = 5.0;
	[Export]
	protected double _inputSensitivity = .0025;

	[Export]
	protected ShapeCast3D FocusCast;

	[ExportGroup("Grabbing")]
	[Export]
	protected double _carryGrabSpeedMultiplier = 1.0f;
	[Export]
	protected double _carryMovementSpeedMultiplier = 0.5f;

	private InputManager _inputManager;
	private double _gravity;


	[ExportGroup("Stealth")]
	[Export]
	protected float _stealthCapsuleHeight;
	[Export]
	protected Vector3 _stealthCapsulePosition;
	[Export]
	protected SoundComponent _soundComponent;

	protected Timer _footstepTimer;

	[ExportGroup("Combat")]
	[Export]
	public CombatGridComponent CombatGrid { get; protected set; }


	public Node3D _currentFloor;
	private bool _isStealthMode = false;
	private float _defaultCapsuleHeight;
	private Vector3 _defaultCapsulePosition;

	// private FocusInfo currentFocus;
	// private object _currentPlayerFocusCollider;

	public override void _Ready()
	{
		// get gravity project settings
		_gravity = (double)ProjectSettings.GetSetting("physics/3d/default_gravity");

		// setup player in global data
		Global.Data.Player = this;

		// ensure there is an input manager before binding events to it
		if (GetNode("/root/InputManager") is InputManager m)
		{
			_inputManager = m;
			_inputManager.MouseMove += HandleMouseMove;
			_inputManager.Sprint += CharacterMovement.ToggleSprint;
			_inputManager.Jump += CharacterMovement.Jump;
			_inputManager.Stealth += ToggleStealth;
			_inputManager.Interact += (isAlt) =>
			{
				if (_interactionComponent.CanInteract)
				{
					_interactionComponent.Interact(isAlt);
				}
			};

			_inputManager.Use += (modifierPressed) =>
			{
				if (_equipmentManager.EquippedItem != null)
				{
					_equipmentManager.EquippedItem.GetNode<Interaction>("Interaction")?.Use(this);
				}
			};
		}

		if (_soundComponent != null)
		{
			_footstepTimer = new()
			{
				WaitTime = .7f,
				Autostart = true,
				OneShot = false
			};

			_footstepTimer.Timeout += () =>
			{
				_soundComponent.EmitSound();
			};

			AddChild(_footstepTimer);
			_footstepTimer.Start();
		}

		FocusCast.AddException(this);

		Debug.Assert(_soundComponent != null, "no sound component");

		// get our default capsule settings for crouching
		CapsuleShape3D bodyCapsule = (CapsuleShape3D)CharacterBody.Shape;
		_defaultCapsuleHeight = bodyCapsule.Height;
		_defaultCapsulePosition = CharacterBody.Position;
	}

	public override void _PhysicsProcess(double delta)
	{

		if (FocusCast != null)
		{
			CheckPlayerFocus();
		}

		if (!IsOnFloor())
		{
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
			HitResult traceResult = Trace.Line(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, x);

			if (traceResult != null && traceResult.Collider is Node3D n)
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


		Vector2 inputDirection = InputManager.GetInputDirection();
		Velocity = CharacterMovement.CalculateMovementVelocity(inputDirection, delta);

		_ = MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	private void CheckPlayerFocus()
	{
		if (!FocusCast.IsColliding() || FocusCast.CollisionResult.Count == 0)
		{
			if (currentFocus != null)
			{
				LoseFocus();
			}

			return;
		}

		Array<Node3D> ColliderResults = new();

		for (int i = 0; i < FocusCast.CollisionResult.Count; i++)
		{
			ColliderResults.Add((Node3D)FocusCast.GetCollider(i));
		}

		if (currentFocus == null)
		{
			foreach (Node3D node in ColliderResults)
			{
				if (CheckCanFocus(node))
				{
					SetFocus(node);
					break;
				}
			}
		}
		else
		{
			if (ColliderResults.Contains(currentFocus))
			{
				return;
			}

			LoseFocus();
		}
	}

	private bool CheckCanFocus(Node3D targetObject)
	{
		float distanceToTargetObject = (PrimaryCamera.GlobalPosition - targetObject.GlobalPosition).Length();

		if (targetObject == this || targetObject == null)
		{
			return false;
		}

		if ((targetObject.HasNode("Interaction") || targetObject is RigidBody3D) && distanceToTargetObject < 1.5)
		{

			return true;
		}


		return false;
	}

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

			RotateY(headRelative);
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
		CharacterMovement.ToggleSneak();

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
}