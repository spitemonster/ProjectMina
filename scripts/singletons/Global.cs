using Godot;
using Godot.Collections;
using ProjectMina.EQS;

namespace ProjectMina;
public partial class Global : Node
{
	[Signal] public delegate void PlayerSetEventHandler(PlayerCharacter player);
	[Signal] public delegate void AICharacterAddedEventHandler(AICharacter character);
	public static Global Data { get; private set; }

	public static EnvironmentQuerySystem EQS;
	
	public PlayerCharacter Player { get; set; }
	public Array<AICharacter> AICharacters { get; set; } = new();
	public MainScene MainScene { get; set; }
	public LevelBase CurrentLevel { get; set; }
	public AudioManager AudioManager { get; set; }

	public bool SetPlayer(PlayerCharacter player)
	{
		if (Player != null)
		{
			return false;
		}

		Player = player;
		EmitSignal(SignalName.PlayerSet, Player);
		return true;
	}

	public bool SetEnvironmentQuerySystem(EnvironmentQuerySystem eqs)
	{
		if (EQS != null)
		{
			return false;
		}

		EQS = eqs;
		return true;
	}

	public bool AddAICharacter(AICharacter newCharacter)
	{
		if (AICharacters.Contains(newCharacter))
		{
			return false;
		}

		AICharacters.Add(newCharacter);
		return true;
	}

	public override void _EnterTree()
	{
		if (Data != null)
		{
			QueueFree();
		}
		Data = this;
	}
}