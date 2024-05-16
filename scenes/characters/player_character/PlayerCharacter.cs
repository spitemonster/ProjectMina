using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class PlayerCharacter : CharacterBase
{
	[ExportGroup("PlayerCharacter")]
	[Export] public Camera3D PrimaryCamera { get; protected set; }
	[Export] public Viewmodel PlayerViewmodel { get; protected set; }
	[Export] public GrabbingComponent GrabComponent { get; protected set; }
	[Export] public VisibilityComponent VisibilityComponent { get; protected set; }
	
	[Export] protected Node3D _head;
	[ExportGroup("Grabbing")]
	
	[Export] protected double CarryGrabSpeedMultiplier = 1.0f;
	[Export] protected SoundComponent _soundComponent;

	[ExportGroup("Combat")]
	[Export] public CombatGridComponent CombatGrid { get; protected set; }

	[Export] public AICharacter TestAiCharacter;

	public Vector3 CameraForwardVector;
	

	private Node3D _currentFloor;
	private bool _isStealthMode = false;
	private float _defaultCapsuleHeight;
	private Vector3 _defaultCapsulePosition;

	private AnimationLibrary _attackAnimations;
	private AnimationLibrary _weaponEquippedAnimations;
	private AnimationLibrary _toolEquippedAnimations;
	private AnimationLibrary _idleAnimations;

	private AnimationNodeBlendTree _animTreeRoot;

	private AnimationNodeAnimation _rightArmBasePose;
	private AnimationNodeAnimation _rightHandBasePose;
	private AnimationNodeAnimation _leftArmBasePose;
	private AnimationNodeAnimation _leftHandBasePose;

	private Godot.Collections.Array<Rid> x = new();

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
			GD.Print("this is the culprit, isn't it");
			QueueFree();
			return;
		}

		if (Global.Data.AICharacters.Count > 0)
		{
			TestAiCharacter = Global.Data.AICharacters[0];
			GD.Print("added test ai character");
		}

		Global.Data.Player = this;
		
		base._Ready();
		
		x.Add(GetRid());

		CallDeferred("InitEvents");		

		// AnimPlayer?.Play("head_idle");
		CharacterAttention?.AddExclude(this);
		
		// get our default capsule settings for crouching
		var bodyCapsule = (CapsuleShape3D)CharacterBody.Shape;
		_defaultCapsuleHeight = bodyCapsule.Height;
		_defaultCapsulePosition = CharacterBody.Position;
		_animTreeRoot = (AnimationNodeBlendTree)PlayerViewmodel.AnimTree.TreeRoot;
		
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
		CharacterMovement.SneakStarted += _StartStealth;
		CharacterMovement.SneakEnded += _EndStealth;
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
					if (TestAiCharacter == null && Global.Data.AICharacters.Count > 0)
					{
						TestAiCharacter = Global.Data.AICharacters[0];
					}
					GD.Print("tested and working");
					TestAiCharacter?.SetTargetPosition(GlobalPosition);
				}
				break;
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
			GD.Print();
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
				CharacterMovement.JumpReleased();
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
		if (PlayerViewmodel.Camera != null)
		{
			CameraForwardVector = -PlayerViewmodel.Camera.GlobalBasis.Z;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

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
			var traceResult = Cast.Ray(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, x);
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

		var controlInput = PlayerInput.GetInputDirection();
		var direction = (GlobalTransform.Basis * new Vector3(controlInput.X, 0, controlInput.Y)).Normalized();
		
		Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, spaceState, _floorSurface);

		_ = MoveAndSlide();

		PlayerViewmodel.MovementSpeed = Velocity.Length();
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
		_head.RotateX(cameraRelative);

		Vector3 clampedCameraRotation = new()
		{
			X = Mathf.Clamp(_head.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80))
		};

		_head.Rotation = clampedCameraRotation;
	}

	public override void Attack()
	{
		AnimationNodeAnimation attackNode = (AnimationNodeAnimation)_animTreeRoot.GetNode("attack");
		attackNode.Animation = _GetAttackAnimation();
		PlayerViewmodel.AnimTree.Set("parameters/right_arm_one_shot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
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
		characterSaveData.CameraTransform = _head.GlobalTransform;
		
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
		CharacterMovement.SneakStarted -= _StartStealth;
		CharacterMovement.SneakEnded -= _EndStealth;
		CharacterEquipment.WeaponEquipped -= _OnWeaponEquipped;
		CharacterEquipment.WeaponUnequipped -= _OnWeaponUnequipped;
		GD.Print("REMOVING PLAYER CHARACTER");
		Global.Data.Player = null;
		GetParent().RemoveChild(this);
		QueueFree();
	}

	public void Load(SaveDataBase saveData)
	{
		if (saveData is SaveDataPlayer playerSave)
		{
			GlobalTransform = playerSave.Transform;
			_head.GlobalTransform = playerSave.CameraTransform;
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
		_rightHandBasePose = (AnimationNodeAnimation)_animTreeRoot.GetNode("right_hand_base_pose");
		_attackAnimations = weapon.GetUseAnimations(true);
		_weaponEquippedAnimations = weapon.GetEquippedAnimations(true);
		
		PlayerViewmodel.AnimPlayer.AddAnimationLibrary("weapon_attacks", _attackAnimations);
		PlayerViewmodel.AnimPlayer.AddAnimationLibrary("weapon_equipped", _weaponEquippedAnimations);

		_InitWeaponAnimations();
		// PlayerViewmodel.AnimPlayer.AddAnimationLibrary("idle", _idleAnimations);
	}

	private void _OnWeaponUnequipped()
	{
		PlayerViewmodel.AnimPlayer.RemoveAnimationLibrary("weapon");
		_attackAnimations = null;
		PlayerViewmodel.AnimTree.Set("parameters/right_hand_one_shot/blend_amount", 0.0f);
		_rightHandBasePose.Animation = null;
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

		GD.Print("no anim found");
		return "";
	}

	private void _InitWeaponAnimations()
	{
		_rightHandBasePose.Animation = "weapon_equipped/idle";
		
		PlayerViewmodel.AnimTree.Set("parameters/right_hand/blend_amount", 1.0);
	}
}