using Godot;

namespace ProjectMina;
public partial class Global : Node
{
	private static Global _instance;
	public static Global Data => _instance;

	public PlayerCharacter Player
	{
		get => _player;
		set => _player = value;
	}

	public MainScene MainScene
	{
		get => _mainScene;
		set => _mainScene = value;
	}

	public LevelBase CurrentLevel
	{
		get => _currentLevel;
		set => _currentLevel = value;
	}

	public DevUI DevLog
	{
		get => _devLog;
		set => _devLog = value;
	}

	public AudioManager AudioManager;

	public override void _EnterTree()
	{
		if (_instance != null)
		{
			QueueFree();
		}
		_instance = this;
	}

	private PlayerCharacter _player;

	private MainScene _mainScene;

	private LevelBase _currentLevel;

	private DevUI _devLog;

}