using Godot;

public partial class LightSwinger : Node3D
{
	[Export] protected bool Swing = false;
	[Export] protected AnimationPlayer AnimPlayer;
	public override void _Ready()
	{
		if (Swing)
		{
			AnimPlayer?.Play("swing");
		}
	}
}
