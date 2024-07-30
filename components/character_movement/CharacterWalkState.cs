using Godot;
using System;

namespace ProjectMina;

public partial class CharacterWalkState : CharacterMovementState
{
    [Export] protected float WalkSpeed = 1.0f;
    [Export] protected float AccelerationForce = 0.1f;

    public override void EnterState(StringName previousState)
    {
        AnimStateMachine.Travel("Movement");
        AnimTree.Set("parameters/movement_anims/Movement/walk_run/blend_position", 0);
    }
	
    public override Vector3 TickMovement(Vector2 movementInput, double delta)
    {
        var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f; 
        
        Vector3 newVelocity = new()
        {
            X = Mathf.MoveToward(Character.Velocity.X, movementInput.X * WalkSpeed, AccelerationForce * frictionMultiplier),
            Y = Character.Velocity.Y,
            Z = Mathf.MoveToward(Character.Velocity.Z, movementInput.Y * WalkSpeed, AccelerationForce * frictionMultiplier)
        };

        return newVelocity;
    }

    public override void ExitState(StringName nextState)
    {
    }
}