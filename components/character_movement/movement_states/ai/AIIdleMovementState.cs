using Godot;
using System;

namespace ProjectMina;
[GlobalClass]
public partial class AIIdleMovementState : AIMovementState
{
	[Export] protected float BrakingForce = 0.1f;
	[Export] protected bool SprintBrakingReduction = true;
	[Export] protected float SprintBrakingReductionMultiplier = .5f;

	private float _brakingForce;
	
	public override void EnterState(StringName previousState)
	{
		var currentAnimState = AnimStateMachineRoot.GetCurrentNode();
		GD.Print(currentAnimState);
		GD.Print($"parameters/{currentAnimState}/playback");

		if (currentAnimState == "")
		{
			return;
		}
		
		var currentStateMachine = (AnimationNodeStateMachinePlayback)AnimTree.Get($"parameters/{currentAnimState}/playback");
		
		currentStateMachine.Travel("idle");

		_brakingForce = BrakingForce;

		if (previousState == "Sprint" && SprintBrakingReduction)
		{
			_brakingForce *= SprintBrakingReductionMultiplier;
		}
	}
	
	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		var currentAnimState = AnimStateMachineRoot.GetCurrentNode();
		// GD.Print(currentAnimState);
		// GD.Print($"parameters/{currentAnimState}/playback");
		var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f;
		
		Vector3 newVelocity = new()
		{
			X = Mathf.MoveToward(Character.Velocity.X, 0, _brakingForce * frictionMultiplier),
			Y = Character.Velocity.Y,
			Z = Mathf.MoveToward(Character.Velocity.Z, 0, _brakingForce * frictionMultiplier)
		};

		return newVelocity;
	}
}
