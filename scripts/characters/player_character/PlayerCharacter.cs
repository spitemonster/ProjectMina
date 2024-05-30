using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class CharacterControlContext : GodotObject
{
	public Vector2 ControlInput = Vector2.Zero;
	public Vector3 Velocity = Vector3.Zero;
	public bool Falling => Velocity.Y < 0;
	public bool Grounded = true;
	public bool WishSprint = false;
	public bool WishJump = false;
	public bool WishCrouch = false;
	public bool WishClimb = false;
}

public partial class PlayerCharacter : CharacterBase
{
	[ExportGroup("PlayerCharacter")]
	[Export] public Camera3D PrimaryCamera { get; protected set; }
	[Export] public Camera3D ViewmodelCamera { get; protected set; }
	[Export] public Viewmodel PlayerViewmodel { get; protected set; }
	[Export] public Node3D Head { get; protected set; }
	
	[ExportGroup("Grabbing")]
	[Export] public GrabbingComponent GrabComponent { get; protected set; }
	[Export] protected double CarryGrabSpeedMultiplier = 1.0f;
	
	[ExportGroup("Visibility")]
	[Export] public VisibilityComponent VisibilityComponent { get; protected set; }
	
	[ExportGroup("Movement")]
	[Export] public CharacterMovementStateMachine MovementStateMachine;

	public Vector3 CameraForwardVector { get; private set; }
	private Node3D _currentFloor;
	private bool _isStealthMode = false;

	private AnimationLibrary _attackAnimations;
	private AnimationLibrary _weaponEquippedAnimations;
	private AnimationLibrary _toolEquippedAnimations;
	private AnimationLibrary _idleAnimations;

	private AnimationNodeBlendTree _animTreeRoot;

	private AnimationNodeAnimation _rightArmBasePose;
	private AnimationNodeAnimation _rightHandBasePose;
	private AnimationNodeAnimation _leftArmBasePose;
	private AnimationNodeAnimation _leftHandBasePose;

	// use this to ignore the player when doing ray casts. probably a better way.
	private Array<Rid> _exclude = new();

	private CharacterControlContext _controlContext = new();

	private bool _wishSprint = false;

	public float GetVisibility()
	{
		if (VisibilityComponent != null)
		{
			return VisibilityComponent.GetVisibility();
		}

		return 1;
	}
	public override void _Ready()
	{
		// setup player in global data
		if (Global.Data.Player != null)
		{
			QueueFree();
			return;
		}

		Global.Data.SetPlayer(this);
		
		base._Ready();
		
		_exclude.Add(GetRid());

		CallDeferred("InitEvents");		

		CharacterAttention?.AddExclude(this);
		
		// get our default capsule settings for crouching
		var bodyCapsule = (CapsuleShape3D)CharacterBody.Shape;
		PrimaryCamera.MakeCurrent();
	}

	private void InitEvents()
	{
		PlayerInput.Manager.MouseMove += HandleMouseMove;
		PlayerInput.Manager.ActionPressed += _OnActionPressed;
		PlayerInput.Manager.ActionReleased += _OnActionReleased;
		PlayerInput.Manager.ActionHoldStarted += _OnActionHoldStarted;
		PlayerInput.Manager.ActionHoldCompleted += _OnActionHoldCompleted;
		PlayerInput.Manager.ActionHoldCanceled += _OnActionHoldCanceled;
		// CharacterMovement.SneakStarted += _StartStealth;
		// CharacterMovement.SneakEnded += _EndStealth;
		CharacterEquipment.WeaponEquipped += _OnWeaponEquipped;
		CharacterEquipment.WeaponUnequipped += _OnWeaponUnequipped;
	}

	private void _OnActionPressed(StringName action)
	{
		switch (action)
		{
			case "use":
				if (CharacterEquipment.HasWeapon())
				{
					Attack();
				}
				else
				{
					CharacterInteraction.Interact();
				}
				
				break;
			case "run":
				// CharacterMovement.ToggleSprint();
				// _wishSprint = true;
				MovementStateMachine.RequestTransition(
					MovementStateMachine.CurrentState == "Sprint" ? "Walk" : "Sprint");
				break;
			case "jump":
				MovementStateMachine.RequestTransition("Jump");
				break;
			case "stealth":
				// CharacterMovement.ToggleSneak();
				_controlContext.WishCrouch = true;
				break;
			case "interact":
				if (GrabComponent.IsGrabbing())
				{
					GrabComponent.ReleaseGrabbedItem();	
				}
				else
				{
					CharacterInteraction.Interact();
				}
				break;
		}
	}

	private void _OnActionHoldStarted(StringName action)
	{
		// if the character is holding on interact it means they're trying to grab something
		if (action == "interact" && CharacterAttention.CurrentFocus != null)
		{
			// if we can't grab the item, interact with it as normal and prevent the hold from executing
			if (!GrabbingComponent.CanGrab(CharacterAttention.CurrentFocus) &&
			    CharacterInteraction.CanInteract())
			{
				CharacterInteraction.Interact();
				PlayerInput.Manager.ClearActionHold(action);
			}

		}
	}

	private void _OnActionHoldCompleted(StringName action)
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

	private void _OnActionReleased(StringName action, bool actionCompleted)
	{
		switch (action)
		{
			case "jump":
				// CharacterMovement.JumpReleased();
				break;
		}
	}

	private void _OnActionHoldCanceled(StringName action, float completedRatio)
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
		CameraForwardVector = -PrimaryCamera.GlobalBasis.Z;
		if (ViewmodelCamera != null)
		{
			ViewmodelCamera.GlobalTransform = PrimaryCamera.GlobalTransform;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var spaceState = GetWorld3D().DirectSpaceState;
		
		if (!IsOnFloor())
		{
			if (_currentFloor is RigidBody3D r)
			{
				r.Sleeping = false;
			}
			_currentFloor = null;
			_floorSurface = null;
		}
		else
		{
			var traceResult = Cast.Ray(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, _exclude);
			_floorSurface = GetFloorSurface(CharacterBody.GlobalPosition);
			
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

		// TODO: hmmmmm....
		// var controlInput = PlayerInput.GetInputDirection();
		// var direction = (GlobalTransform.Basis * new Vector3(controlInput.X, 0, controlInput.Y)).Normalized();
		
		// Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, spaceState, _floorSurface);
		
		// _controlContext.ControlInput = PlayerInput.GetInputDirection();
		// _controlContext.Velocity = Velocity;
		// _controlContext.Grounded = IsOnFloor();
		// _controlContext.WishClimb = false;
		// _controlContext.WishSprint = _wishSprint;
		
		MovementStateMachine.ApplyCharacterControl(this, _controlContext, delta);
		MoveAndSlide();
		base._PhysicsProcess(delta);

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
		Head.RotateX(cameraRelative);

		Vector3 clampedCameraRotation = new()
		{
			X = Mathf.Clamp(Head.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80))
		};

		Head.Rotation = clampedCameraRotation;
	}

	public override void Attack()
	{
		// AnimationNodeAnimation attackNode = (AnimationNodeAnimation)_animTreeRoot.GetNode("attack");
		// attackNode.Animation = _GetAttackAnimation();
	}

	public override void Footstep()
	{
		
		if (_floorSurface != default)
		{
			switch (_floorSurface.ResourceName)
			{
				case "Grass":
					break;
				case "Wood":
					break;
				case "Ice":
					break;
				default:
					break;
			}
		}
	}

	public void Save(SavedGame savedGame)
	{
		var characterSaveData = new SaveDataPlayer();

		characterSaveData.ScenePath = SceneFilePath;
		characterSaveData.Position = GlobalPosition;
		characterSaveData.Transform = GlobalTransform;
		characterSaveData.CameraTransform = Head.GlobalTransform;
		
		savedGame.GameData.Add(characterSaveData);
	}

	public void BeforeLoad()
	{
		PlayerInput.Manager.MouseMove -= HandleMouseMove;
		PlayerInput.Manager.ActionPressed -= _OnActionPressed;
		PlayerInput.Manager.ActionReleased -= _OnActionReleased;
		PlayerInput.Manager.ActionHoldStarted -= _OnActionHoldStarted;
		PlayerInput.Manager.ActionHoldCompleted -= _OnActionHoldCompleted;
		PlayerInput.Manager.ActionHoldCanceled -= _OnActionHoldCanceled;
		
		// CharacterMovement.SneakStarted -= _StartStealth;
		// CharacterMovement.SneakEnded -= _EndStealth;
		CharacterEquipment.WeaponEquipped -= _OnWeaponEquipped;
		CharacterEquipment.WeaponUnequipped -= _OnWeaponUnequipped;
		
		Global.Data.Player = null;
		GetParent().RemoveChild(this);
		QueueFree();
	}

	public void Load(SaveDataBase saveData)
	{
		if (saveData is SaveDataPlayer playerSave)
		{
			GlobalTransform = playerSave.Transform;
			Head.GlobalTransform = playerSave.CameraTransform;
		}
	}
	
	
	// TODO: when the player is crouching (actively moving between one state and another) line of sight detection doesn't work
	// since it casts to character chest which is outside of the crouch capsule for a few frames.  
	private void _StartStealth()
	{
		CharacterAnimationPlayer.Play("crouch");
		CharacterBody.Disabled = true;
	}

	private void _EndStealth()
	{
		CharacterAnimationPlayer.PlayBackwards("crouch");
		CharacterBody.Disabled = false;
	}
	
	private void _OnWeaponEquipped(EquippableComponent weapon)
	{

		_InitWeaponAnimations();
		// PlayerViewmodel.AnimPlayer.AddAnimationLibrary("idle", _idleAnimations);
	}

	private void _OnWeaponUnequipped()
	{
	}

	private void _GetWeaponIdleAnimation()
	{
	}

	private StringName _GetAttackAnimation()
	{
		if (_attackAnimations != null)
		{
			var animName = "weapon_attacks/" + _attackAnimations.GetAnimationList().PickRandom();
			return animName;
		}
		
		return "";
	}

	private void _InitWeaponAnimations()
	{
	}

	public override void _ExitTree()
	{
		_controlContext.Free();
		base._ExitTree();
	}

	private void _ClearMovementContext()
	{
		_controlContext.ControlInput = Vector2.Zero;
		_controlContext.Velocity = Vector3.Zero;
		_controlContext.Grounded = true;
		_controlContext.WishSprint = false;
		_controlContext.WishJump = false;
		_controlContext.WishCrouch = false;
		_controlContext.WishClimb = false;
	}
}