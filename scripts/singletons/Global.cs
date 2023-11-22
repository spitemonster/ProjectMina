using Godot;

namespace ProjectMina;
public partial class Global : Node
{
	public static Global Data { get; private set; }
	public PlayerCharacter Player { get; set; }
	public MainScene MainScene { get; set; }
	public LevelBase CurrentLevel { get; set; }
	public DevUI DevLog { get; set; }
	public AudioManager AudioManager { get; set; }

	public override void _EnterTree()
	{
		if (Data != null)
		{
			QueueFree();
		}
		Data = this;
	}
}