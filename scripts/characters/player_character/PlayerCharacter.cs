using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class PlayerCharacter : CharacterBase
{
	[ExportGroup("PlayerCharacter")]
	[Export] public Camera3D PrimaryCamera { get; protected set; }
	
	[Export] protected EquipmentManager _equipmentManager;
	
	[Export] protected Node3D _head;

	[ExportGroup("Grabbing")]
	[Export] public GrabbingComponent GrabComponent { get; protected set; }
	[Export] protected double CarryGrabSpeedMultiplier = 1.0f;
	
	[Export] protected SoundComponent _soundComponent;

	[ExportGroup("Combat")]
	[Export] public CombatGridComponent CombatGrid { get; protected set; }

	public Vector3 CameraForwardVector;
	

	private Node3D _currentFloor;
	private bool _isStealthMode = false;
	private float _defaultCapsuleHeight;
	private Vector3 _defaultCapsulePosition;

	private Godot.Collections.Array<Rid> x = new();
	public override void _Ready()
	{
		base._Ready();
		
		// setup player in global data
		Global.Data.Player = this;

		x.Add(GetRid());

		CallDeferred("InitEvents");		

		AnimPlayer?.Play("head_idle");
		CharacterAttention?.AddExclude(this);
		
		// get our default capsule settings for crouching
		var bodyCapsule = (CapsuleShape3D)CharacterBody.Shape;
		_defaultCapsuleHeight = bodyCapsule.Height;
		_defaultCapsulePosition = CharacterBody.Position;
	}

	private void InitEvents()
	{
		PlayerInput.Manager.MouseMove += HandleMouseMove;
		PlayerInput.Manager.ActionPressed += OnActionPressed;
		PlayerInput.Manager.ActionReleased += OnActionReleased;
		PlayerInput.Manager.ActionHoldStarted += OnActionHoldStarted;
		PlayerInput.Manager.ActionHoldCompleted += OnActionHoldCompleted;
		PlayerInput.Manager.ActionHoldCanceled += OnActionHoldCanceled;
		CharacterMovement.SneakStarted += StartStealth;
		CharacterMovement.SneakEnded += EndStealth;
	}

	private void OnActionPressed(StringName action)
	{
		switch (action)
		{
			case "run":
				CharacterMovement.ToggleSprint();
				break;
			case "jump":
				CharacterMovement.TryJump();
				break;
			case "stealth":
				CharacterMovement.ToggleSneak();
				break;
			case "interact":
				if (GrabComponent.IsGrabbing())
				{
					GrabComponent.ReleaseGrabbedItem();	
				}
				break;
		}
	}

	private void OnActionHoldStarted(StringName action)
	{
		// if the character is holding on interact it means they're trying to grab something
		if (action == "interact" && CharacterAttention.CurrentFocus != null)
		{
			// if we can't grab the item, interact with it as normal and prevent the hold from executing
			if (!GrabbingComponent.CanGrab(CharacterAttention.CurrentFocus) &&
			    CharacterInteraction.CanInteract(CharacterAttention.CurrentFocus))
			{
				CharacterInteraction.Interact(CharacterAttention.CurrentFocus);
				PlayerInput.Manager.ClearActionHold(action);
			}

		}
	}

	private void OnActionHoldCompleted(StringName action)
	{
		if (action == "interact")
		{
			if (GrabComponent.IsGrabbing())
			{
				GrabComponent.ReleaseGrabbedItem(CameraForwardVector * 500.0f);
				return;
			}

			if (CharacterAttention.CurrentFocus != null && GrabbingComponent.CanGrab(CharacterAttention.CurrentFocus))
			{
				GrabComponent.GrabItem((RigidBody3D)CharacterAttention.CurrentFocus);
			}
		}
	}

	private void OnActionReleased(StringName action, bool actionCompleted)
	{
		switch (action)
		{
			case "jump":
				CharacterMovement.JumpReleased();
				break;
		}
	}

	private void OnActionHoldCanceled(StringName action, float completedRatio)
	{
		if (action == "interact")
		{
			if (GrabComponent.IsGrabbing())
			{
				GrabComponent.ReleaseGrabbedItem(CameraForwardVector * 500.0f, completedRatio);
				return;
			}
			
			if (CharacterAttention.CurrentFocus != null && GrabbingComponent.CanGrab(CharacterAttention.CurrentFocus))
			{
				GrabComponent.GrabItem((RigidBody3D)CharacterAttention.CurrentFocus);
			}
		}
	}

	public override void _Process(double delta)
	{
		if (PrimaryCamera != null)
		{
			CameraForwardVector = -PrimaryCamera.GlobalBasis.Z;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var spaceState = GetWorld3D().DirectSpaceState;

		// if (_equipmentManager.EquippedItem is RangedWeapon w && _equipmentManager.EquippedItemType == Equipment.EquipmentType.Weapon)
		// {
		// 	var traceStart = PrimaryCamera.GlobalPosition;
		// 	var traceEnd = PrimaryCamera.GlobalPosition + PrimaryCamera.GlobalTransform.Basis.Z * -100.0f;
		// 	var aimTraceResult = Trace.Line(spaceState, traceStart, traceEnd, x);
		// 	var aimPosition = aimTraceResult?.HitPosition ?? traceEnd;
		//
		// 	w.Aim(aimPosition);
		// }

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
			var traceResult = Trace.Line(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, x);

			if (traceResult is { Collider: RigidBody3D r })
			{
				r.Sleeping = true;
				_currentFloor = r;
			}
		}

		// iterate over all collisions and apply collision physics to them
		for (var i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision3D = GetSlideCollision(i);
			if (collision3D.GetCollider() is not RigidBody3D r || !IsOnFloor() || r == _currentFloor) continue;
			
			var directionToCollision = (r.GlobalPosition - CharacterBody.GlobalPosition).Normalized();
			var angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
			var angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
			angleFactor = Mathf.Round(angleFactor * 100) / 100;

			if (!(angleFactor > 0.5)) continue;
			
			r.ApplyCentralImpulse(-collision3D.GetNormal() * 4.0f * angleFactor);
			r.ApplyImpulse(-collision3D.GetNormal() * 0.01f * angleFactor, collision3D.GetPosition());
		}

		var controlInput = PlayerInput.GetInputDirection();
		var direction = (GlobalTransform.Basis * new Vector3(controlInput.X, 0, controlInput.Y)).Normalized();
		Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, spaceState);

		_ = MoveAndSlide();
	}

	private void HandleMouseMove(Vector2 mouseRelative)
	{
		if (GetTree().Paused) return;
		
		var headRelative = (float)(-mouseRelative.X * .0025);
		var cameraRelative = (float)(-mouseRelative.Y * .0025);

		if (GrabComponent.IsGrabbing())
		{
			headRelative *= (float)CarryGrabSpeedMultiplier;
			cameraRelative *= (float)CarryGrabSpeedMultiplier;
		}

		RotateY(headRelative);
		PrimaryCamera.RotateX(cameraRelative);

		Vector3 clampedCameraRotation = new()
		{
			X = Mathf.Clamp(PrimaryCamera.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80))
		};

		PrimaryCamera.Rotation = clampedCameraRotation;
	}

	private void StartStealth()
	{
		AnimPlayer.Play("crouch");
		CharacterBody.Disabled = true;
	}

	private void EndStealth()
	{
		AnimPlayer.PlayBackwards("crouch");
		CharacterBody.Disabled = false;
	}
}
