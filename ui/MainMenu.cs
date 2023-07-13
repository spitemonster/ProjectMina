using Godot;
using ProjectMina;

namespace ProjectMina;

public partial class MainMenu : Control
{
	private InputManager _inputManager;
	private bool _isShown;

	public bool IsShown { get; }

	[Export]
	protected Button _returnButton;
	[Export]
	protected Button _quitButton;

	public override void _Ready()
	{
		_isShown = Visible;

		if (GetNode("/root/InputManager") is InputManager i)
		{
			_inputManager = i;
		}

		if (_returnButton != null)
		{
			_returnButton.Pressed += ToggleMainMenu;
		}

		Debug.Assert(_returnButton != null, "no return button");

		if (_quitButton != null)
		{
			_quitButton.Pressed += () => GetTree().Quit();
		}

		Debug.Assert(_quitButton != null, "no quit button");
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
		_isShown = !_isShown;
		Visible = _isShown;
		Global.Data.MainScene.SetPause(_isShown);
		InputManager.SetMouseCapture(!_isShown);
	}
}