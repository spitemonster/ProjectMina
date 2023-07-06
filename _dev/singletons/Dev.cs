using Godot;

namespace ProjectMina;
public partial class Dev : Node
{
	public static Dev Core { get; private set; }
	public static DevUI UI { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		if (Global.Data != null)
		{
			UI = new DevUI();
			Global.Data.DevLog = UI;
			GetNode("/root/MainScene").AddChild(UI);
		}
	}
}