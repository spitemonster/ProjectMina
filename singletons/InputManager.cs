using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class InputManager : Node
{
	[Signal] public delegate void MouseMoveEventHandler(Vector2 mouseDelta);
	[Signal] public delegate void LookEventHandler(Vector2 lookDelta);
	[Signal] public delegate void UseEventHandler(bool isAlt);
	[Signal] public delegate void EndUseEventHandler();
	[Signal] public delegate void InteractEventHandler(bool isAlt);
	[Signal] public delegate void InteractReleasedEventHandler();
	[Signal] public delegate void SprintEventHandler();
	[Signal] public delegate void JumpEventHandler();
	[Signal] public delegate void JumpPressedEventHandler();
	[Signal] public delegate void JumpReleasedEventHandler();
	[Signal] public delegate void StealthEventHandler();
	[Signal] public delegate void ReloadEventHandler();
	[Signal] public delegate void LeanLeftEventHandler();
	[Signal] public delegate void LeanLeftReleasedEventHandler();
	[Signal] public delegate void LeanRightEventHandler();
	[Signal] public delegate void LeanRightReleasedEventHandler();

	[Signal] public delegate void LeanEventHandler(uint direction, bool end);
	public bool ModifierPressed { get => _modifierPressed; }
	public InputManager Instance { get => this; }

	private Timer _leftMouseTimer;
	private bool _leftMouseClicked = false;
	private bool _leftMouseDoubleClicked = false;

	private Timer _rightMouseTimer;
	private bool _rightMouseClicked;
	private bool _rightMouseDoubleClicked;
	private bool _modifierPressed;

	private double _holdDuration = 1.0;

	[Export] protected double _doubleClickWaitTime = .09;

	private LabelValueRow _modMonitor;

	public override void _Ready()
	{
		CallDeferred("InitPreview");
	}

	private void InitPreview()
	{
		// _modMonitor = Dev.UI.AddDevMonitor("Alt Depressed");
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
			// _modMonitor.SetValue(_modifierPressed.ToString());
			return;
		}

		if (e.IsActionPressed("use"))
		{
			EmitSignal(SignalName.Use, _modifierPressed);
			return;
		}

		if (e.IsActionReleased("use"))
		{
			EmitSignal(SignalName.EndUse);
			return;
		}

		if (e.IsActionPressed("interact"))
		{
			EmitSignal(SignalName.Interact, _modifierPressed);
			return;
		}
		else if (e.IsActionReleased("interact"))
		{
			EmitSignal(SignalName.InteractReleased);
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
			EmitSignal(SignalName.JumpPressed);
			return;
		}
		else if (e.IsActionReleased("jump"))
		{
			EmitSignal(SignalName.JumpReleased);
			return;
		}

		if (e.IsActionPressed("stealth"))
		{
			EmitSignal(SignalName.Stealth);
			return;
		}

		if (e.IsActionPressed("reload"))
		{
			EmitSignal(SignalName.Reload);
			return;
		}

		if (e.IsActionPressed("lean_left"))
		{
			EmitSignal(SignalName.Lean, (uint)Enums.DirectionHorizontal.Left, false);
			return;
		}
		else if (e.IsActionReleased("lean_left"))
		{
			EmitSignal(SignalName.Lean, (uint)Enums.DirectionHorizontal.Left, true);
		}

		if (e.IsActionPressed("lean_right"))
		{
			EmitSignal(SignalName.Lean, (uint)Enums.DirectionHorizontal.Right, false);
			return;
		}
		else if (e.IsActionReleased("lean_right"))
		{
			EmitSignal(SignalName.Lean, (uint)Enums.DirectionHorizontal.Right, true);
			return;
		}
	}

	static public void SetMouseCapture(bool shouldCapture)
	{
		Input.MouseMode = shouldCapture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
	}

	static public Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_back");

		return inputDirection;
	}
}