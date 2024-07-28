using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class PlayerControllerComponent : ControllerComponent
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerInput.Manager.MouseMove += _HandleMouseMove;
	}
	
	private void _HandleMouseMove(Vector2 mouseRelative)
	{
		Pawn.SetControlInput(mouseRelative);
	}

	public override void _Process(double delta)
	{
		Pawn.SetMovementInput(PlayerInput.GetInputDirection());
	} 
}
