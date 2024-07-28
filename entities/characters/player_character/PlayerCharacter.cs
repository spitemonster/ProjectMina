using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using ProjectMina.EnvironmentQuerySystem;

namespace ProjectMina;
public partial class PlayerCharacter : CharacterBase
{
	[Export] public Camera3D PrimaryCamera { get; protected set; }
	
	[ExportSubgroup("Grabbing")]
	[Export] public GrabbingComponent GrabComponent { get; protected set; }
	[Export] protected double CarryGrabSpeedMultiplier = 1.0f;
	
	[ExportSubgroup("Visibility")]
	[Export] public VisibilityComponent VisibilityComponent { get; protected set; }
	
	[ExportSubgroup("Movement")]
	[Export] public CharacterMovementStateMachine MovementStateMachine;
	
	public AnimationNodeTimeScale MovementAnimsTimeScale { get; protected set; }
	
	public float LookSpeedMultiplier = 1;
	public Vector3 CameraForwardVector { get; private set; }

	// use this to ignore the player when doing ray casts. probably a better way.
	private Array<Rid> _exclude = new();
	
	private DevMonitor _floorMonitor;
	private DevMonitor _floorSurfaceMonitor;

	private RayCast3D _aimCast;
	
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

		CharacterAttention?.AddExclude(this);

		PrimaryCamera.MakeCurrent();

		AnimTree.Active = true;

		_aimCast = GetNode<RayCast3D>("%AimCast");
		
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
	}

	private void _OnActionPressed(StringName action)
	{
		switch (action)
		{
			case "use":
				var start = PrimaryCamera.GlobalPosition;
				// for (int i = 0; i < 12; i++)
				// {
				// 	var randVec = Utilities.Math.GetRandomConeVector(CameraForwardVector, 3f);
				// 	var end = start + randVec * 25.0f;
				// 	Cast.Ray(GetWorld3D().DirectSpaceState, start, end, new() {GetRid()}, true, true, 10.0f);
				// }
				if (CharacterEquipment.IsWeaponEquipped())
				{
					CharacterEquipment.UseWeapon();
				}
				break;
			case "equip":
				if (CharacterAttention.CurrentFocus != null &&
				    CharacterEquipment.CanEquip(CharacterAttention.CurrentFocus) is { } f)
				{
					CharacterEquipment.Equip(f);
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
				MovementStateMachine.RequestTransition(MovementStateMachine.CurrentState == "Crouch" ? "Walk" : "Crouch");
				// _controlContext.WishCrouch = true;
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
				if (CharacterEquipment.IsWeaponEquipped())
				{
					CharacterEquipment.ReloadWeapon();
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
				GrabComponent.ReleaseGrabbedItem(CameraForwardVector * 10.0f);
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
				break;
			case "lean_left":
				MovementStateMachine.RequestTransition("Idle");
				break;
			case "use":
				if (CharacterEquipment.IsWeaponEquipped())
				{
					CharacterEquipment.EndUseWeapon();
				}
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
		base._Process(delta);
		CameraForwardVector = -PrimaryCamera.GlobalBasis.Z;
		
		var headRelative = (float)(-ControlInput.X * .0025);
		var cameraRelative = (float)(-ControlInput.Y * .0025);

		RotateY(headRelative);
		Head.RotateX(cameraRelative);

		Head.Rotation = _ClampHeadYaw(Head.Rotation.X);
		ControlInput = Vector2.Zero;
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
		if (CharacterEquipment.IsWeaponEquipped() && CharacterEquipment.IsRangedWeaponEquipped())
		{
			Vector3 aimPosition = Vector3.Zero;
			var weapon = (RangedWeaponComponent)CharacterEquipment.GetWeapon();
			if (_aimCast.IsColliding())
			{
				aimPosition = _aimCast.GetCollisionPoint();
			}
			else
			{
				aimPosition = _aimCast.TargetPosition + _aimCast.GlobalPosition;
			}
			
			weapon.Aim(aimPosition);
		}
		
		base._PhysicsProcess(delta);
		
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