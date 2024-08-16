using System;
using Godot;


namespace ProjectMina;

public partial class MainScene : Control
{

	[Signal] public delegate void LevelLoadedEventHandler(LevelBase level); 
	
	[Export] protected bool EnableDebug = true;
	[Export] protected Node3D LevelSlot;
	[Export] protected PackedScene StartingLevel;
	public LevelBase CurrentLevel { get; private set; }

	public void Save(SavedGame savedGame)
	{
		SaveDataGlobal global = new()
		{
			CurrentLevel = CurrentLevel.SceneFilePath,
		};

		savedGame.GlobalData = global;
	}

	public void BeforeLoad()
	{
		// UnloadCurrentLevel();
	}

	public void Load(SavedGame savedGame)
	{
		LoadLevel(GD.Load<PackedScene>(savedGame.GlobalData.CurrentLevel));
	}

	public bool LoadLevel(PackedScene levelScene)
	{
		if (LevelSlot == null || levelScene == null)
		{
			if (EnableDebug) GD.PushError("Main scene missing slot or level is null");
			return false;
		}

		if (!UnloadCurrentLevel())
		{
			if (EnableDebug) GD.PushError("Main scene failed unloading the current level");
		}

		var level = ResourceLoader.Load<PackedScene>(levelScene.ResourcePath);

		if (level.Instantiate() is LevelBase l)
		{
			LevelSlot.AddChild(l);
			CurrentLevel = l;
			Global.Data.CurrentLevel = CurrentLevel;
			EmitSignal(SignalName.LevelLoaded, CurrentLevel);
			return true;
		}

		if (EnableDebug) GD.PushError("Attempted to load a non LevelBase level with the main scene.");
		return false;
	}

	public bool UnloadCurrentLevel()
	{
		if (!IsInstanceValid(CurrentLevel) && CurrentLevel != null)
		{
			if (EnableDebug) GD.PushError("Attempted to unload an invalid level from main scene.");
			return false;
		}

		if (CurrentLevel != null)
		{
			CurrentLevel.GetParent().RemoveChild(CurrentLevel);
			CurrentLevel.QueueFree();
			CurrentLevel = null;
		}
		
		return true;
	}
	
	public override void _Ready()
	{
		Global.Data.MainScene = this;

		PlayerInput.SetMouseCapture(true);

		CallDeferred("_Init");
	}

	public override void _PhysicsProcess(double delta)
	{
	}

	private void _Init()
	{
		if (LoadLevel(StartingLevel))
		{
			if (Global.Data.CurrentLevel.PlayerClass != null)
			{
				_SpawnPlayer(Global.Data.CurrentLevel.PlayerClass, Global.Data.CurrentLevel.PlayerStart.GlobalTransform);
			} 
		}
	}
	
	private bool _SpawnPlayer(PackedScene playerClass, Transform3D transform)
	{
		if (Global.Data.CurrentLevel.PlayerClass.Instantiate() is CharacterBody3D c)
		{
			Global.Data.CurrentLevel.AddChild(c);
			c.GlobalTransform = transform;

			return true;
		}
	
		return false;
	}
}