using Godot;
using Godot.Collections;
using ProjectMina;

public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; }
	public static bool SaveGame()
	{
		var savedGame = new SavedGame();

		Global.Data.MainScene.GetTree().CallGroup("GameEvents", "Save", savedGame);

		_SaveGameData(savedGame);
		return true;
	}

	public bool LoadGame()
	{
		if (ResourceLoader.Load("res://savedata.tres") is not SavedGame savedGame)
		{
			GD.PushError("Error loading save data");
			return false;
		}
		
		_Preload();

		CallDeferred("_LoadGameData", savedGame);
		return true;
	}

	private static bool _SaveGameData(SavedGame savedGame)
	{
		ResourceSaver.Save(savedGame, "res://savedata.tres");
		return false;
	}

	private static bool _Preload()
	{
		Global.Data.MainScene.GetTree().CallGroup("GameEvents", "BeforeLoad");
		return false;
	}

	private static bool _LoadGameData(SavedGame savedGame)
	{
		Global.Data.MainScene.GetTree().CallGroup("GameEvents", "Load", savedGame);

		var gameData = savedGame.GameData;
		
		foreach (var saveData in gameData)
		{
			PackedScene scene = GD.Load<PackedScene>(saveData.ScenePath);
		
			var node = scene?.Instantiate();
		
			if (node != null)
			{
				Global.Data.CurrentLevel.AddChild(node);
				
				if (node.HasMethod("Load"))
				{
					node.Call("Load", saveData);
				}	
			}
			else
			{
				GD.PushError("Attempted to load an invalid scene instance.");
			}
		}
		
		return false;
	}
	
	public override void _EnterTree()
	{
		if (Instance == null)
		{
			Instance = this;
			return;
		}

		QueueFree();
	}
}
