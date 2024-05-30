using Godot;
using System;
using ProjectMina;

public partial class PowerSwitch : Node3D
{
	[Export] public bool SwitchState = false;
	[Export] protected MeshInstance3D OnButtonMesh;
	[Export] protected MeshInstance3D OffButtonMesh;
	
	[Export] private AnimationPlayer _animPlayer; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// OnButtonInteraction.InteractionStarted += _SwitchOn;
		// OffButtonInteraction.InteractionStarted += _SwitchOff;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _SwitchOn(CharacterBase character)
	{
	}

	private void _SwitchOff(CharacterBase character)
	{
	}

	private void _TogglePower(bool state)
	{
		// if (state)
		// {
		// 	_animPlayer.PlayBackwards("switch");
		// }
		// else
		// {
		// 	_animPlayer.Play("switch");
		// }
	}
}
