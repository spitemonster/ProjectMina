using Godot;

namespace ProjectMina;
public partial class Dev : Node
{
	public static Dev Core { get; private set; }
	public static DevUI UI { get; private set; }
	private static readonly string _scenePath = "res://_dev/ui/DevUI.tscn";

	public override void _EnterTree()
	{
		if (Core != null)
		{
			QueueFree();
		}
		Core = this;
	}

	public override void _Ready()
	{
		base._Ready();

		if (Global.Data != null)
		{
			if (ResourceLoader.Exists(_scenePath) && ResourceLoader.Load(_scenePath) is PackedScene s)
			{
				CallDeferred("InitializeUI", s);
			}
		}
	}


	private void InitializeUI(PackedScene hudScene)
	{
		DevUI devHUD = hudScene.Instantiate<DevUI>();
		if (devHUD != null)
		{
			GetTree().Root.GetNode("MainScene").AddChild(devHUD);
			UI = devHUD;
		}
	}
}