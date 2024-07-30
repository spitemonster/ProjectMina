using Godot;

namespace ProjectMina;

public partial class PlayerJumpState : PlayerMovementState
{
	[Export] protected float JumpForce = 2.0f;
	[Export] protected float JumpPeakTime = .5f;
	[Export] protected float JumpHeight = 1f;
	[Export] protected float JumpDistance = 3f;

	private float _speed = 0f;
	private float _jumpVelocity = 0f;

	private float _jumpGravity = -9.8f;
	
	[Export(PropertyHint.Range, "0,1,0.05")] protected float AirControlMultiplier = 0.5f;

	public override void _Ready()
	{
		base._Ready();
		_jumpGravity = (2 * JumpHeight) / Mathf.Pow(JumpPeakTime, 2);
		_jumpVelocity = _jumpGravity * JumpPeakTime;
	}
	
	public override void EnterState(StringName previousState)
	{
		if (previousState == "Sprint")
		{
			MovementStateMachine.CharacterSprintJumped = true;
		}
		
		var newVelocity = Character.Velocity;
		newVelocity.Y = _jumpVelocity;

		Character.Velocity = newVelocity;
		AnimStateMachine.Travel("Jump");
	}

	public override Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		if (Character.Velocity.Y < 0.0)
		{
			
			EmitSignal(State.SignalName.Transition, "Falling");
			return Character.Velocity;
		}
		var controlInput = PlayerInput.GetInputDirection();
		var direction = (Character.GlobalTransform.Basis * new Vector3(controlInput.X, 0, controlInput.Y)).Normalized();
		var velocity = new Vector3()
		{
			X = Character.Velocity.X + ((direction.X * AirControlMultiplier * (float)delta)),
			Y = Character.Velocity.Y - _jumpGravity * (float)delta,
			Z = Character.Velocity.Z + ((direction.Y * AirControlMultiplier * (float)delta))
		};

		return velocity;
	}
}