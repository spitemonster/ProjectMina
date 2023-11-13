using Godot;
using Godot.Collections;
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

	[Signal] public delegate void TestEventHandler();
	[Signal] public delegate void TestHoldFinishedEventHandler();

	[Signal] public delegate void LeanEventHandler(uint direction, bool end);

	[Export] public float HoldTriggerDuration = 0.1f;
	[Export] public float HoldDuration = 1.0f;
	// [Export] public Array
	
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

	private InputManagerSettings settings;
	// TESTING
	private Timer _testTimer;
	
	private Array<StringName> _holdTimeoutActions = new();
	private Array<StringName> _heldActions = new();
	private Array<StringName> _completedActions = new();
	
	[Signal] public delegate void ActionPressedEventHandler(StringName action);
	[Signal] public delegate void ActionReleasedEventHandler(StringName action, bool actionCompleted);
	[Signal] public delegate void ActionHoldStartedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCompletedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCanceledEventHandler(StringName action);

	private Array<StringName> _actions;
	
	public override void _Ready()
	{
		CallDeferred("InitPreview");

		_actions = InputMap.GetActions();

		settings = (InputManagerSettings)ResourceLoader.Load("res://settings/InputSettings.tres");
	}

	public override void _Process(double delta)
	{
		foreach (var action in _actions)
		{
			if (Input.IsActionJustPressed(action))
			{
				if (settings.HoldableActions.Contains(action))
				{
					HandleActionPress(action);
				}
				else
				{
					EmitSignal(SignalName.ActionPressed, action);
				}
			} else if (Input.IsActionJustReleased(action))
			{
				HandleActionRelease(action);
			}
		}
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
		}
	}

	private void HandleActionPress(StringName action)
	{
		GD.Print("Input Press Started");
		Timer holdTimeoutTimer = new()
		{
			WaitTime = .1f,
			Autostart = false,
			OneShot = true
		};

		holdTimeoutTimer.Timeout += () =>
		{
			if (_holdTimeoutActions.Contains(action))
			{
				_holdTimeoutActions.Remove(action);
				HandleActionHoldStart(action);	
			}
			
			holdTimeoutTimer.QueueFree();
		};
		
		GetTree().Root.AddChild(holdTimeoutTimer);
		_holdTimeoutActions.Add(action);
		holdTimeoutTimer.Start();
	}

	public void ClearActionHold(StringName action)
	{
		_holdTimeoutActions.Remove(action);
		_heldActions.Remove(action);
		_completedActions.Remove(action);
	}

	private void HandleActionHoldStart(StringName action)
	{
		_heldActions.Add(action);

		Timer holdTimer = new()
		{
			Autostart = false,
			WaitTime = 1.0f,
			OneShot = true
		};

		holdTimer.Timeout += () =>
		{
			if (_heldActions.Contains(action))
			{
				HandleActionHoldComplete(action);
			}
			
			holdTimer.QueueFree();
		};
		
		GetTree().Root.AddChild(holdTimer);
		holdTimer.Start();
		EmitSignal(SignalName.ActionHoldStarted, action);
		GD.Print("Input Hold Started: ", action);
	}

	private void HandleActionHoldCancel(StringName action)
	{
		_heldActions.Remove(action);
		EmitSignal(SignalName.ActionHoldCanceled, action);
		GD.Print("Input hold canceled: ", action);
	}

	private void HandleActionHoldComplete(StringName action)
	{
		_heldActions.Remove(action);
		_completedActions.Add(action);
		EmitSignal(SignalName.ActionHoldCompleted, action);
		GD.Print("input hold complete: ", action);
	}

	private void HandleActionRelease(StringName action)
	{
		if (_holdTimeoutActions.Contains(action))
		{
			GD.PushError("input hold was less than hold trigger time executing default action: ", action);
			_holdTimeoutActions.Remove(action);
			EmitSignal(SignalName.ActionPressed, action);
		} else if (_heldActions.Contains(action))
		{
			EmitSignal(SignalName.ActionReleased, action, false);
			HandleActionHoldCancel(action);
		} else
		{
			bool actionCompleted = _completedActions.Contains(action);
			
			if (!actionCompleted)
			{
				_completedActions.Remove(action);
			}
			
			GD.Print("action released: ", action, ", action completed: ", actionCompleted);
			EmitSignal(SignalName.ActionReleased, action, actionCompleted);
		}
	}

	public static void SetMouseCapture(bool shouldCapture)
	{
		Input.MouseMode = shouldCapture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
	}

	public static Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_back");

		return inputDirection;
	}
}