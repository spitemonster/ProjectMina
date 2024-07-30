using Godot;
using System;

namespace ProjectMina;

public partial class PlayerLeanState : PlayerMovementState
{
    private int numFramesTicked;
    public override void EnterState(StringName previousState)
    {
    }
	
    public override Vector3 TickMovement(Vector2 movementInput, double delta)
    {
        numFramesTicked++;
        return Vector3.Zero;
    }

    public override void ExitState(StringName nextState)
    {
        
    }
}
