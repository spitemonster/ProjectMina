using Godot;
using System;

namespace ProjectMina;

public partial class PlayerGrabState : PlayerMovementState
{
	[Export] protected float BaseWalkSpeed = 1.0f;
	[Export] protected float AccelerationForce = 0.1f;
	[Export] protected float MaxCarryWeight = 100.0f;
	[Export] protected Curve SpeedMultiplierCurve;
	[Export] protected Curve LookMultiplierCurve;

	private float _speedMultiplier = .5f;
	private float _lookMultiplier = .5f;
	private float _defaultLookMultiplier;

	public override void EnterState(StringName previousState)
	{
		var massRatio = Mathf.Min(Player.GrabComponent.GrabbedItem.Mass / MaxCarryWeight, 1f);
		_speedMultiplier = SpeedMultiplierCurve.Sample(massRatio);
		_lookMultiplier = LookMultiplierCurve.Sample(massRatio);
		_defaultLookMultiplier = Player.LookSpeedMultiplier;

		Player.LookSpeedMultiplier = _lookMultiplier;
	}
	
	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		var frictionMultiplier = Character.CurrentFloorMaterial?.Friction ?? 1.0f;
		var speed = BaseWalkSpeed * _speedMultiplier;
		
		Vector3 direction = (Character.GlobalTransform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
		
		Vector3 newVelocity = new()
		{
			X = Mathf.MoveToward(Character.Velocity.X, direction.X * speed, AccelerationForce * frictionMultiplier),
			Y = Character.Velocity.Y,
			Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * speed, AccelerationForce * frictionMultiplier)
		};

		return newVelocity;
	}

	public override void ExitState(StringName nextState)
	{
		Player.LookSpeedMultiplier = _defaultLookMultiplier;
	}
}
