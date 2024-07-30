using Godot;
using System;

namespace ProjectMina;

public partial class CharacterFallingState : CharacterMovementState
{
	[Export(PropertyHint.Range, "0,1,0.05")] protected float AirControlMultiplier = 0.5f;
	
	public override void EnterState(StringName previousState)
	{
		// MovementAnimStateMachine.Travel("Falling");
	}

	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		if (Character.IsOnFloor() || Character.Velocity.Y >= 0.0)
		{
			EmitSignal(State.SignalName.Transition, "Landed");
			return Character.Velocity;
		}
		
		var direction = (Character.GlobalTransform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
		
		var velocity = new Vector3()
		{
			X = Character.Velocity.X + ((direction.X * AirControlMultiplier * (float)delta)),
			Y = Character.Velocity.Y,
			Z = Character.Velocity.Z + ((direction.Y * AirControlMultiplier * (float)delta))
		};

		return velocity;
	}
}
