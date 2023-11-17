using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class Growler : Label
{

	[Export]
	protected AnimationPlayer _animPlayer;
	
	public override void _Ready()
	{
		Timer testTimer = new()
		{
			WaitTime = 3.0,
			Autostart = true,
			OneShot = true
		};

		testTimer.Timeout += QueueFree;

		AddChild(testTimer);
	}
}
