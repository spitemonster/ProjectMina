using Godot;


namespace ProjectMina
{
	public partial class MainScene : Control
	{

		[Export] protected bool EnableDebug = false;
		[Export] protected Node3D LevelSlot;
		[Export] protected Resource StartingLevel;
		
		public LevelBase CurrentLevel { get; private set; }

		public bool LoadLevel(Resource level)
		{
			if (LevelSlot == null || level == null)
			{
				if (EnableDebug) GD.PushError("Main scene missing slot or level is null");
				return false;
			}

			if (!UnloadCurrentLevel())
			{
				if (EnableDebug) GD.PushError("Main scene failed unloading the current level");
			}

			PackedScene levelScene = GD.Load<PackedScene>(level.ResourcePath);

			if (levelScene.Instantiate() is LevelBase l)
			{
				LevelSlot.AddChild(l);
				CurrentLevel = l;
				Global.Data.CurrentLevel = CurrentLevel;
				return true;
			}
			else
			{
				if (EnableDebug) GD.PushError("Attempted to load a non LevelBase level with the main scene.");
				return false;
			}
		}
		
		public override void _Ready()
		{
			Global.Data.MainScene = this;

			PlayerInput.Manager.SetMouseCapture(true);

			LoadLevel(StartingLevel);
		}

		private bool UnloadCurrentLevel()
		{
			if (!IsInstanceValid(CurrentLevel) && CurrentLevel != null)
			{
				if (EnableDebug) GD.PushError("Attempted to unload an invalid level from main scene.");
				return false;
			}

			if (CurrentLevel != null)
			{
				LevelSlot.RemoveChild(CurrentLevel);
				CurrentLevel.QueueFree();
				CurrentLevel = null;
			}
			
			return true;
		}
	}
}
