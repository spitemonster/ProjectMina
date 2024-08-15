using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using ProjectMina.EQS;

namespace ProjectMina;
public partial class PlayerCharacter : CharacterBase
{

	[Signal] public delegate void WeaponEquippedEventHandler(EWeaponType weaponType);
	
	[Export] public Camera3D PrimaryCamera { get; protected set; }
	
	[ExportSubgroup("Grabbing")]
	[Export] public GrabbingComponent GrabComponent { get; protected set; }
	[Export] protected double CarryGrabSpeedMultiplier = 1.0f;
	
	[ExportSubgroup("Visibility")]
	[Export] public VisibilityComponent VisibilityComponent { get; protected set; }
	
	[ExportSubgroup("Movement")]
	[Export] public CharacterMovementStateMachine MovementStateMachine;

	[Export] protected ShapeCast3D AttentionCast;
	
	public AnimationNodeTimeScale MovementAnimsTimeScale { get; protected set; }
	public float LookSpeedMultiplier = 1;
	public Vector3 CameraForwardVector { get; private set; }
	// use this to ignore the player when doing ray casts. probably a better way.
	private Array<Rid> _exclude = new();
	private DevMonitor _floorMonitor;
	private DevMonitor _floorSurfaceMonitor;
	private RayCast3D _aimCast;

	private Node3D _currentFocus;

	private PlayerAnimationComponent _animationComponent;
	
	public override float GetVisibility()
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
		
		_floorMonitor = Dev.UI.AddDevMonitor("Floor: ", Colors.Orange, "Player");
		_floorSurfaceMonitor = Dev.UI.AddDevMonitor("Floor Material: ", Colors.Orange, "Player");
		
		_exclude.Add(GetRid());

		PrimaryCamera.MakeCurrent();

		AnimTree.Active = true;

		_aimCast = GetNode<RayCast3D>("%AimCast");

		_animationComponent = GetNode<PlayerAnimationComponent>("%AnimationComponent");
		
		CallDeferred("InitEvents");
	}

	private void InitEvents()
	{
		PlayerInput.Manager.ActionPressed += _OnActionPressed;
		PlayerInput.Manager.ActionReleased += _OnActionReleased;
		PlayerInput.Manager.ActionHoldStarted += _OnActionHoldStarted;
		PlayerInput.Manager.ActionHoldCompleted += _OnActionHoldCompleted;
		PlayerInput.Manager.ActionHoldCanceled += _OnActionHoldCanceled;
		
		GrabComponent.ItemGrabbed += (Node3D item) =>
		{
			MovementStateMachine.RequestTransition("Grab");
		};

		GrabComponent.ItemDropped += (Node3D item, bool fomBreak) =>
		{
			if (Velocity.Length() > 0)
			{
				MovementStateMachine.RequestTransition("Walk");
			}
			else
			{
				MovementStateMachine.RequestTransition("Idle");
			}
		};

		MovementStateMachine.StateTransitioned += (StringName newState, StringName previousState) =>
		{
			
		};

		CharacterEquipment.WeaponEquipped += (weaponComponent) =>
		{
			EmitSignal(SignalName.WeaponEquipped, (int)weaponComponent.WeaponType);
		};
	}

	private void _OnActionPressed(StringName action)
	{
		switch (action)
		{
			case "use":
				if (CharacterEquipment.IsWeaponEquipped && CanAttack)
				{
					_animationComponent.PlayAttack();
				}
				break;
			case "equip":
				if (_currentFocus != null && _currentFocus is RigidBody3D r && 
				    EquipmentComponent.CanEquip(r))
				{
					CharacterEquipment.Equip(r);
				}
				break;
			case "run":
				MovementStateMachine.RequestTransition(
					MovementStateMachine.CurrentState == "Sprint" ? "Walk" : "Sprint");
				break;
			case "jump":
				MovementStateMachine.RequestTransition("Jump");
				break;
			case "stealth":
				MovementStateMachine.RequestTransition(MovementStateMachine.CurrentState == "Crouch" ? "Walk" : "Crouch");
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
			case "lean_left":
				MovementStateMachine.RequestTransition("Lean");
				break;
			case "reload":
				if (CharacterEquipment.IsWeaponEquipped)
				{
					_animationComponent.PlayReload();
				}
				else
				{
					GD.Print("problem!");
				}
				break;
		}
	}
	
	private void _OnActionReleased(StringName action, bool actionCompleted)
	{
		switch (action)
		{
			case "jump":
				break;
			case "lean_left":
				MovementStateMachine.RequestTransition("Idle");
				break;
			case "use":
				if (CharacterEquipment.IsWeaponEquipped && CharacterEquipment.EquippedWeapon is RangedWeaponComponent)
				{
					CharacterEquipment.EndUseWeapon();
				}
				break;
		}
	}

	private void _OnActionHoldStarted(StringName action)
	{
		// if the character is holding on interact it means they're trying to grab something
		if (action == "interact" && _currentFocus != null)
		{
			// if we can't grab the item, interact with it as normal and prevent the hold from executing
			if (!GrabbingComponent.CanGrab(_currentFocus) &&
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
				GrabComponent.ReleaseGrabbedItem(CameraForwardVector * 10.0f);
				return;
			}


			if (_currentFocus != null && GrabbingComponent.CanGrab(_currentFocus))
			{
				GrabComponent.GrabItem((RigidBody3D)_currentFocus);
			}
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
			
			if (_currentFocus != null && GrabbingComponent.CanGrab(_currentFocus))
			{
				GrabComponent.GrabItem((RigidBody3D)_currentFocus);
			}
		}
	}

	public override void Attack()
	{
		base.Attack();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		CameraForwardVector = -PrimaryCamera.GlobalBasis.Z;
		
		var headRelative = (float)(-ControlInput.X * .0025);
		var cameraRelative = (float)(-ControlInput.Y * .0025);

		RotateY(headRelative);
		Head.RotateX(cameraRelative);

		Head.Rotation = _ClampHeadYaw(Head.Rotation.X);
		ControlInput = Vector2.Zero;

		if (CharacterEquipment.EquippedWeapon is RangedWeaponComponent r)
		{
			var target = new Vector3();
		
			_aimCast.ForceRaycastUpdate();
			if (_aimCast.IsColliding())
			{
				target = _aimCast.GetCollisionPoint();
			}
			else
			{
				target = -PrimaryCamera.GlobalBasis.Z * 15f + PrimaryCamera.GlobalPosition;
			}
			
			r.Aim(target);
		}
	}

	private Vector3 _ClampHeadYaw(float baseHeadYaw, float min = -80f, float max = 80f)
	{
		Vector3 clampedYaw = new()
		{
			X = Mathf.Clamp(baseHeadYaw, Mathf.DegToRad(min), Mathf.DegToRad(max))
		};

		return clampedYaw;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		_floorMonitor?.SetValue(CurrentFloor != null ? CurrentFloor.Name : "None");
		_floorSurfaceMonitor?.SetValue(CurrentFloorMaterial != null ? CurrentFloorMaterial.ResourceName : "None");
		
		Velocity = MovementStateMachine.GetCharacterVelocity(MovementInput, delta);

		AttentionCast.ForceShapecastUpdate();

		Node3D focus = null;
		
		Array<Node3D> colliderResults = new();

		for (var i = 0; i < AttentionCast.CollisionResult.Count; i++)
		{
			if (AttentionCast.GetCollider(i) is Node3D n)
			{
				colliderResults.Add(n);
			}
		}
		
		foreach (var result in colliderResults)
		{
			if (!_TargetHasFocusableNode(result) && result is not RigidBody3D) continue;
			focus = result;
			break;
		}

		_SetFocus(focus);
		
		base._PhysicsProcess(delta);
	}

	private void _SetFocus(Node3D target)
	{
		if (target == _currentFocus)
		{
			return;
		}
		
		if (target == null && _currentFocus != null)
		{
			_BreakFocus();
			return;
		}
		
		EmitSignal(SignalName.FocusChanged, target, _currentFocus);
		_currentFocus = target;
		
		if (_GetTargetInteractableComponent(_currentFocus) is { } n)
		{
			n.ReceiveFocus(this);
		}
	}

	private void _BreakFocus()
	{
		// EmitSignal(SignalName.FocusBroken, _currentFocus);
		EmitSignal(SignalName.FocusChanged, (Node3D)null, _currentFocus );
		if (_GetTargetInteractableComponent(_currentFocus) is { } n)
		{
			n.LoseFocus();
		}
		
		_currentFocus = null;
	}

	private static bool _TargetHasFocusableNode(Node3D target)
	{
		return target.HasNode("Interactable") || target.HasNode("Equippable") || target.HasNode("Tool") ||
		       target.HasNode("Weapon") || target.HasNode("RangedWeapon") || target.HasNode("MeleeWeapon");
	}
	
	private static InteractableComponent _GetTargetInteractableComponent(Node3D target)
	{
		string[] nodeTests = {"Interactable", "Equippable", "Tool", "Weapon", "RangedWeapon", "MeleeWeapon", "Usable"};

		foreach (var test in nodeTests)
		{
			if (target.GetNodeOrNull(test) is InteractableComponent i)
			{
				return i;
			}
		}

		return null;
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
		PlayerInput.Manager.ActionPressed -= _OnActionPressed;
		PlayerInput.Manager.ActionReleased -= _OnActionReleased;
		PlayerInput.Manager.ActionHoldStarted -= _OnActionHoldStarted;
		PlayerInput.Manager.ActionHoldCompleted -= _OnActionHoldCompleted;
		PlayerInput.Manager.ActionHoldCanceled -= _OnActionHoldCanceled;
		
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
	public void Crouch()
	{
		Body.Disabled = true;
	}

	public void EndCrouch()
	{
		Body.Disabled = false;
	}
}