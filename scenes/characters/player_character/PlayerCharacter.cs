using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class PlayerCharacter : CharacterBase
{

	[Export]
	protected RangedWeapon gun;

	[ExportGroup("PlayerCharacter")]
	[Export]
	public Camera3D PrimaryCamera { get; protected set; }
	[Export] public AnimationPlayer AnimationPlayer { get; protected set; }

	[Export]
	protected InteractionComponent _interactionComponent;
	[Export]
	protected EquipmentManager _equipmentManager;
	[Export]
	protected Node3D _head;
	[Export]
	protected double _inputSensitivity = .0025;

	[Export]
	protected ShapeCast3D FocusCast;

	[ExportGroup("Grabbing")]
	[Export]
	protected double _carryGrabSpeedMultiplier = 1.0f;

	private InputManager _inputManager;

	[ExportGroup("Stealth")]
	[Export]
	protected float _stealthCapsuleHeight;
	[Export]
	protected Vector3 _stealthCapsulePosition;
	[Export]
	protected SoundComponent _soundComponent;

	[ExportGroup("Combat")]
	[Export] public CombatGridComponent CombatGrid { get; protected set; }

	public Node3D _currentFloor;
	private bool _isStealthMode = false;
	private float _defaultCapsuleHeight;
	private Vector3 _defaultCapsulePosition;

	private Godot.Collections.Array<Rid> x = new();

	// private FocusInfo CharacterAttention.CurrentFocus;
	// private object _currentPlayerFocusCollider;

	public override void _Ready()
	{
		// setup player in global data
		Global.Data.Player = this;

		x.Add(GetRid());

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

			_inputManager.EndUse += () =>
			{
				if (_equipmentManager.EquippedItem != null)
				{
					_equipmentManager.EquippedItem.GetNode<Interaction>("Interaction")?.EndUse(this);
				}
			};

			_inputManager.Reload += () =>
			{
				if (_equipmentManager.EquippedItem != null && _equipmentManager.EquippedItemType == Equipment.EquipmentType.Weapon && _equipmentManager.EquippedItem is RangedWeapon w)
				{
					w.Reload();
				}
			};

			_inputManager.Lean += CharacterMovement.Lean;
		}

		FocusCast.AddException(this);

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

		if (_equipmentManager.EquippedItem is RangedWeapon w && _equipmentManager.EquippedItemType == Equipment.EquipmentType.Weapon)
		{
			Vector3 traceStart = PrimaryCamera.GlobalPosition;
			Vector3 traceEnd = PrimaryCamera.GlobalPosition + PrimaryCamera.GlobalTransform.Basis.Z * -100.0f;
			HitResult aimTraceResult = Trace.Line(GetWorld3D().DirectSpaceState, traceStart, traceEnd, x);
			Vector3 aimPosition = aimTraceResult != null ? aimTraceResult.HitPosition : traceEnd;

			// DebugDraw.Sphere(aimPosition, .25f, Colors.Red, .5f);

			w.Aim(aimPosition);
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
			HitResult traceResult = Trace.Line(GetWorld3D().DirectSpaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, x);

			if (traceResult != null && traceResult.Collider is Node3D n)
			{
				if (n is RigidBody3D r)
				{
					_currentFloor = r;
					// r.Sleeping = true;
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

				if (angleFactor > 0.5)
				{
					r.ApplyCentralImpulse(-collision3D.GetNormal() * 4.0f * angleFactor);
					r.ApplyImpulse(-collision3D.GetNormal() * 0.01f * angleFactor, collision3D.GetPosition());
				}
			}
		}

		Vector2 controlInput = InputManager.GetInputDirection();
		// Velocity = CharacterMovement.CalculateMovementVelocity(inputDirection, delta);
		// CharacterMovement.AddMovementInput(inputDirection);
		Vector3 direction = (GlobalTransform.Basis * new Vector3(-controlInput.X, 0, -controlInput.Y)).Normalized();
		Velocity = CharacterMovement.GetCharacterVelocity(direction, delta);

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
			if (CharacterAttention.CurrentFocus != null)
			{
				LoseFocus();
			}

			return;
		}

		Array<Node3D> ColliderResults = new();

		for (int i = 0; i < FocusCast.CollisionResult.Count; i++)
		{
			if (FocusCast.GetCollider(i) is Node3D n)
			{
				ColliderResults.Add(n);
			}
		}

		if (CharacterAttention.CurrentFocus == null)
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
			if (ColliderResults != null && ColliderResults.Contains(CharacterAttention.CurrentFocus))
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

		if ((targetObject.HasNode("Interaction") || targetObject is RigidBody3D) && distanceToTargetObject < 2.0)
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

			if (CharacterInteraction.IsGrabbing)
			{

				if (Input.IsActionPressed("mod"))
				{
					CharacterInteraction.AddGrabbedItemRotationSpeed(new Vector3(cameraRelative, headRelative, 0));
					return;
				}

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
			AnimPlayer.Play("crouch");
			CharacterBody.Disabled = true;
		}
		else
		{
			AnimPlayer.PlayBackwards("crouch");
			CharacterBody.Disabled = false;
		}
	}
}