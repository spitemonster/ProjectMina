using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class Growler : Label
{

	[Export]
	protected AnimationPlayer _animPlayer;

	private Debug _debug;

	public override void _Ready()
	{
		// Debug.Assert(_animPlayer != null, "no anim player");
		// _animPlayer?.Play("FadeOut");

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
