using Godot;
using System;

namespace ProjectMina;
public partial class Viewmodel : Node3D
{
	[Export] public Node3D RightHandSocket { get; protected set; }
	[Export] public Node3D LeftHandSocket { get; protected set; }
	[Export] public Camera3D Camera { get; protected set; }
	[Export] public AnimationPlayer AnimPlayer { get; protected set; }
	[Export] public AnimationTree AnimTree { get; protected set; }
	[Export] protected SkeletonIK3D RightHandIK;
	[Export] protected SkeletonIK3D LeftHandIK;
	[Export] protected MovementComponent CharacterMovement;

	[Export] protected double LeanRate = .01;
	[Export] protected double LookRate = .05;

	public float MovementSpeed = 0.0f;
	public float LookVerticalOffset = 0.0f;

	private PlayerCharacter _player;
	private MovementComponent _playerMovementComponent;
	
	private MovementComponent.MovementState _playerMovementState;

	public override void _Ready()
	{
		_player = GetOwner<PlayerCharacter>();
	}

	public void CallFootstep()
	{
		_player.Footstep();
	}

	public override void _PhysicsProcess(double delta)
	{
		RightHandIK?.Start(true);
		LeftHandIK?.Start(true);
		
		if (CharacterMovement != null)
		{
			_playerMovementState = CharacterMovement.CharacterMovementState;
		}
		
		float vLook = Mathf.Clamp(Camera.Rotation.X, -1, 1);
		Vector2 inputDirection = PlayerInput.GetInputDirection();
		Vector2 previousBlendPosition = (Vector2)AnimTree?.Get("parameters/look_lean/blend_position");
		
		float previousLean = previousBlendPosition.X;
		float previousLook = previousBlendPosition.Y;
		
		float nextLean = (float)Mathf.Lerp((double)previousLean, (double)MovementSpeed * inputDirection.X, LeanRate);
		float nextLook = (float)Mathf.Lerp(previousLook, vLook, LookRate);
		
		// AnimTree?.Set("parameters/look_lean/blend_position", new Vector2(nextLean, nextLook));
		// var characterMovementStateMachine = (AnimationNodeStateMachinePlayback)AnimTree?.Get("parameters/character_movement/playback");
		//
		// switch (_playerMovementState)
		// {
		// 	case MovementComponent.MovementState.WALKING:
		// 		characterMovementStateMachine.Travel("walk");
		// 		break;
		// 	case MovementComponent.MovementState.SPRINTING:
		// 		characterMovementStateMachine.Travel("walk");
		// 		break;
		// 	case MovementComponent.MovementState.SNEAKING:
		// 		characterMovementStateMachine.Travel("walk");
		// 		break;
		// 	default:
		// 		characterMovementStateMachine.Travel("idle");
		// 		break;
		// }
	}
}
