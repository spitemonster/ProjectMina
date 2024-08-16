using Godot;
using System;

namespace ProjectMina;

public partial class CharacterLandedState : CharacterMovementState
{
	public override void EnterState(StringName previousState)
	{
		if (MovementStateMachine.CharacterSprintJumped)
		{
			Dev.UI.PushDevNotification("should try to sprint");
			EmitSignal(State.SignalName.Transition, "Sprint");
			MovementStateMachine.CharacterSprintJumped = false;
		}
		else
		{
			EmitSignal(State.SignalName.Transition, "Idle");
		}
	}

	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		return Character.Velocity;
	}
}
