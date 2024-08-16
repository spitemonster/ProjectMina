using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class AIWalkMovementState : AIMovementState
{
    [Export] protected float WalkSpeed = 1.0f;
    [Export] protected float AccelerationForce = 0.1f;

    public override void EnterState(StringName previousState)
    {
        var currentAnimState = AnimStateMachineRoot.GetCurrentNode();

        if (currentAnimState == "")
        {
            return;
        }
		
        CurrentAnimStateMachine = (AnimationNodeStateMachinePlayback)AnimTree.Get($"parameters/{currentAnimState}/playback");
        CurrentAnimStateMachine.Travel("walk");
    }
	
    public override Vector3 TickMovement(Vector2 movementInput, double delta)
    {
        var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f; 
        
        CurrentAnimStateMachine.Set("parameters/walk/blend_position", aiCharacter.LookRotationDeg);
        
        Vector3 newVelocity = new()
        {
            X = Mathf.MoveToward(Character.Velocity.X, movementInput.X * WalkSpeed, AccelerationForce * frictionMultiplier),
            Y = Character.Velocity.Y,
            Z = Mathf.MoveToward(Character.Velocity.Z, movementInput.Y * WalkSpeed, AccelerationForce * frictionMultiplier)
        };

        return newVelocity;
    }
}