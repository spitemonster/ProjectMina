using Godot;

namespace ProjectMina;

public partial class PlayerIdleState : PlayerMovementState
{
	[Export] protected float BrakingForce = 0.1f;
	[Export] protected bool SprintBrakingReduction = true;
	[Export] protected float SprintBrakingReductionMultiplier = .5f;

	private float _brakingForce;
	
	public override void EnterState(StringName previousState)
	{
		AnimStateMachine.Travel("Idle");

		_brakingForce = BrakingForce;

		if (previousState == "Sprint" && SprintBrakingReduction)
		{
			_brakingForce *= SprintBrakingReductionMultiplier;
		}
	}
	
	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
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
