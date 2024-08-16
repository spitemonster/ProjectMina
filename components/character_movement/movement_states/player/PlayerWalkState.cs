using Godot;
using System;

namespace ProjectMina;

public partial class PlayerWalkState : PlayerMovementState
{
	[Export] protected float WalkSpeed = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;

	public override void EnterState(StringName previousState)
	{
		// AnimStateMachineRoot.Travel("Walk");
	}
	
	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f; 
		Vector3 direction = (Character.GlobalTransform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
		
		Vector3 newVelocity = new()
		{
			X = Mathf.MoveToward(Character.Velocity.X, direction.X * WalkSpeed, AccelerationForce * frictionMultiplier),
			Y = Character.Velocity.Y,
			Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * WalkSpeed, AccelerationForce * frictionMultiplier)
		};
		
		AnimTree.SetGlobalFloatParameter("character_movement_speed", newVelocity.Length());

		return newVelocity;
	}

	public override void ExitState(StringName nextState)
	{
	}
}
