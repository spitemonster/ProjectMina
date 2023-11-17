using Godot;
using ProjectMina;

namespace ProjectMina;

public partial class MainMenu : Control
{
	private bool _isShown;

	public bool IsShown { get; }

	[Export]
	protected Button _returnButton;
	[Export]
	protected Button _quitButton;

	public override void _Ready()
	{
		_isShown = Visible;
		
		if (_returnButton != null)
		{
			_returnButton.Pressed += ToggleMainMenu;
		}

		System.Diagnostics.Debug.Assert(_returnButton != null, "no return button");

		if (_quitButton != null)
		{
			_quitButton.Pressed += () => GetTree().Quit();
		}

		System.Diagnostics.Debug.Assert(_quitButton != null, "no quit button");
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
		PlayerInput.Manager.SetPause(_isShown);
		PlayerInput.Manager.SetMouseCapture(!_isShown);
	}
}