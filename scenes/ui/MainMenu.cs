using Godot;
using ProjectMina;

namespace ProjectMina;

public partial class MainMenu : Control
{
	public bool IsShown { get; protected set; }

	[Export] protected Button ReturnButton;
	[Export] protected Button QuitButton;
	[Export] protected Button SaveButton;
	[Export] protected Button LoadButton;

	public override void _Ready()
	{
		IsShown = Visible;
		
		if (ReturnButton != null)
		{
			ReturnButton.Pressed += ToggleMainMenu;
		}

		if (QuitButton != null)
		{
			QuitButton.Pressed += () => GetTree().Quit();
		}

		if (SaveButton != null)
		{
			SaveButton.Pressed += () =>
			{
				SaveManager.SaveGame();
			};
		}

		if (LoadButton != null)
		{
			LoadButton.Pressed += () =>
			{
				SaveManager.Instance.LoadGame();
			};
		}
	}

	public override void _Input(InputEvent e)
	{
		base._Input(e);

		if (e.IsActionPressed("ui_cancel"))
		{
			ToggleMainMenu();
		}
	}

	private void ToggleMainMenu()
	{
		IsShown = !IsShown;
		Visible = IsShown;
		PlayerInput.Manager.SetPause(IsShown);
		PlayerInput.SetMouseCapture(!IsShown);
	}
}