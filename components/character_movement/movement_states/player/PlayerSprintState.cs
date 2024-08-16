using Godot;
using System;

namespace ProjectMina;

public partial class PlayerSprintState : PlayerMovementState
{
	[Export] protected float SprintSpeed = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;

	public override void EnterState(StringName previousState)
	{
		// AnimStateMachineRoot.Travel("Sprint");
		// Player.CharacterAnimationTree.Set("parameters/movement_speed_scale/scale", 0);
	}
	
	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f; 

		Vector3 direction = (Character.GlobalTransform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
		
		Vector3 newVelocity = new()
		{
			X = Mathf.MoveToward(Character.Velocity.X, direction.X * SprintSpeed, AccelerationForce * frictionMultiplier),
			Y = Character.Velocity.Y,
			Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * SprintSpeed, AccelerationForce * frictionMultiplier)
		};

		return newVelocity;
	}

	public override void ExitState(StringName nextState)
	{
		// MovementAnimTimeScale.Set("scale", 1);
		// Player.CharacterAnimationTree.Set("parameters/movement_speed_scale/scale", 1.0);
	}
}
