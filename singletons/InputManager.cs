using Godot;

namespace ProjectMina;

public partial class InputManager : Node
{
	[Signal]
	public delegate void MouseMoveEventHandler(Vector2 mouseDelta);
	[Signal]
	public delegate void UseEventHandler(bool isAlt);
	[Signal]
	public delegate void InteractEventHandler(bool isAlt);
	[Signal]
	public delegate void SprintEventHandler();
	[Signal]
	public delegate void JumpEventHandler();
	[Signal]
	public delegate void StealthEventHandler();

	private Timer _leftMouseTimer;
	private bool _leftMouseClicked = false;
	private bool _leftMouseDoubleClicked = false;

	private Timer _rightMouseTimer;
	private bool _rightMouseClicked;
	private bool _rightMouseDoubleClicked;
	private bool _modifierPressed;

	[Export]
	protected double _doubleClickWaitTime = .09;

	private LabelValueRow _modMonitor;

	public override void _Ready()
	{
		CallDeferred("InitPreview");
	}

	private void InitPreview()
	{
		// _modMonitor = Global.Data.DevLog.AddDevInfo("Alt Depressed");
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion mouseMove)
		{
			EmitSignal(SignalName.MouseMove, mouseMove.Relative);

			return;
		}

		if (e.IsAction("mod"))
		{
			_modifierPressed = e.IsActionPressed("mod");
			_modMonitor.SetValue(_modifierPressed.ToString());
			return;
		}

		if (e.IsActionPressed("use"))
		{
			EmitSignal(SignalName.Use, _modifierPressed);
			return;
		}

		if (e.IsActionPressed("interact"))
		{
			EmitSignal(SignalName.Interact, _modifierPressed);
			return;
		}

		if (e.IsActionPressed("run"))
		{
			EmitSignal(SignalName.Sprint);
			return;
		}

		if (e.IsActionPressed("jump"))
		{
			EmitSignal(SignalName.Jump);
			return;
		}

		if (e.IsActionPressed("stealth"))
		{
			EmitSignal(SignalName.Stealth);
			return;
		}
	}

	static public void SetMouseCapture(bool shouldCapture)
	{
		Input.MouseMode = shouldCapture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
	}

	static public Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		return inputDirection;
	}
}