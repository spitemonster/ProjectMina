using Godot;
using System;

namespace ProjectMina
{
	public partial class MainScene : Node3D
	{

		[Export] protected Node3D _levelSlot;

		[Export] protected Resource _startingLevel;

		private InputManager _inputManager;
		public LevelBase CurrentLevel { get; protected set; }

		public override void _Ready()
		{
			Global.Data.MainScene = this;

			if (GetNode("/root/InputManager") is InputManager m)
			{
				_inputManager = m;
			}

			InputManager.SetMouseCapture(true);

			LoadLevel(_startingLevel);
		}

		public void SetPause(bool shouldPause)
		{
			GetTree().Paused = shouldPause;
		}

		public void LoadLevel(Resource inLevelResource)
		{
			System.Diagnostics.Debug.Assert(_levelSlot != null, "No level slot.");

			UnloadCurrentLevel();

			PackedScene scene = GD.Load<PackedScene>(inLevelResource.ResourcePath);

			if (scene.Instantiate() is LevelBase sceneInstance)
			{
				CurrentLevel = sceneInstance;
				_levelSlot.AddChild(CurrentLevel);

				Global.Data.CurrentLevel = sceneInstance;
			}
			else
			{

			}
		}

		private void UnloadCurrentLevel()
		{
			if (!IsInstanceValid(CurrentLevel))
			{
				return;
			}
			
			_levelSlot.RemoveChild(CurrentLevel);
			CurrentLevel.QueueFree();
			CurrentLevel = null;
		}
	}
}
