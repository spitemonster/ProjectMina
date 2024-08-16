using Godot;

namespace ProjectMina;

public partial class PlayerCrouchState : PlayerMovementState
{
	[Export] protected float CrouchSpeed = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;
	
	private PlayerCharacter _player;
	
	public override void EnterState(StringName previousState)
	{
		PositionAnimStateMachine.Travel("Crouched");

		if (MovementStateMachine.CurrentState == "Sprint")
		{
			MovementStateMachine.RequestTransition("Idle");
		}
	}

	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		Vector3 direction = (Character.GlobalTransform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();

		var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f; 
		
		Vector3 newVelocity = new()
		{
			X = Mathf.MoveToward(Character.Velocity.X, direction.X * CrouchSpeed, AccelerationForce * frictionMultiplier),
			Y = Character.Velocity.Y,
			Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * CrouchSpeed, AccelerationForce * frictionMultiplier)
		};
		
		Character.AnimPlayer.SetSpeedScale(newVelocity.Length() / CrouchSpeed);

		return newVelocity;
	}
	
	public override void ExitState(StringName nextState)
	{
		PositionAnimStateMachine.Travel("Base");
	}
}
