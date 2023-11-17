using Godot;
using Godot.Collections;

namespace ProjectMina;

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
	
	public bool ModifierPressed { get; protected set; }
	public InputManager Instance { get => this; }
	
	private LabelValueRow _modMonitor;

	private InputManagerSettings _settings;
	
	private Array<StringName> _completedActions = new();
	private Dictionary<StringName, Timer> _actionsAwaitingHoldTimers = new();
	private Dictionary<StringName, Timer> _heldActionTimers = new();
	
	[Signal] public delegate void ActionPressedEventHandler(StringName action);
	[Signal] public delegate void ActionReleasedEventHandler(StringName action, bool actionCompleted);
	[Signal] public delegate void ActionHoldStartedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCompletedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCanceledEventHandler(StringName action, float completedRatio);

	private Array<StringName> _actions;
	
	public override void _Ready()
	{
		_actions = InputMap.GetActions();
		_settings = (InputManagerSettings)ResourceLoader.Load("res://resources/settings/InputSettings.tres");
	}

	public override void _Process(double delta)
	{
		foreach (var action in _actions)
		{
			if (Input.IsActionJustPressed(action))
			{
				if (_settings.HoldableActions.Contains(action))
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

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion mouseMove)
		{
			EmitSignal(SignalName.MouseMove, mouseMove.Relative);
		}
	}

	private void HandleActionPress(StringName action)
	{
		Timer holdWaitTimer = new()
		{
			WaitTime = .1f,
			Autostart = false,
			OneShot = true
		};

		holdWaitTimer.Timeout += () => HoldWaitTimeout(action, holdWaitTimer);
		
		GetTree().Root.AddChild(holdWaitTimer);
		_actionsAwaitingHoldTimers.Add(action, holdWaitTimer);
		
		holdWaitTimer.Start();
	}

	private void HoldWaitTimeout(StringName action, Timer waitTimer)
	{
		if (_actionsAwaitingHoldTimers.ContainsKey(action))
		{
			_actionsAwaitingHoldTimers.Remove(action);
			HandleActionHoldStart(action);
		}
		
		waitTimer.QueueFree();
	}

	private void HandleActionHoldStart(StringName action)
	{
		Timer holdTimer = new()
		{
			Autostart = false,
			WaitTime = 1.0f,
			OneShot = true
		};

		holdTimer.Timeout += () => HoldTimeout(action, holdTimer);
		
		GetTree().Root.AddChild(holdTimer);
		_heldActionTimers.Add(action, holdTimer);
		
		holdTimer.Start();
		EmitSignal(SignalName.ActionHoldStarted, action);
	}

	private void HoldTimeout(StringName action, Timer holdTimer)
	{
		if (_heldActionTimers.ContainsKey(action))
		{
			HandleActionHoldComplete(action);
		}
			
		holdTimer.QueueFree();
	}

	private void HandleActionHoldCancel(StringName action)
	{
		var ratio = 0.0f;
		
		if (_heldActionTimers.TryGetValue(action, out var t))
		{
			var timeElapsed = t.WaitTime - t.TimeLeft;
			ratio = (float)(timeElapsed / t.WaitTime);
			
			t.Stop();
			t.QueueFree();
		}
		
		_heldActionTimers.Remove(action);
		EmitSignal(SignalName.ActionHoldCanceled, action, ratio);
	}

	private void HandleActionHoldComplete(StringName action)
	{
		_heldActionTimers.Remove(action);
		_completedActions.Add(action);
		EmitSignal(SignalName.ActionHoldCompleted, action);
	}

	private void HandleActionRelease(StringName action)
	{
		if (_actionsAwaitingHoldTimers.ContainsKey(action))
		{
			Timer t = _actionsAwaitingHoldTimers[action];
			t.Stop();
			t.QueueFree();
			
			_actionsAwaitingHoldTimers.Remove(action);
			EmitSignal(SignalName.ActionPressed, action);
		} else if (_heldActionTimers.ContainsKey(action))
		{
			EmitSignal(SignalName.ActionReleased, action, false);
			HandleActionHoldCancel(action);
		} else
		{
			bool actionCompleted = _completedActions.Contains(action);

			if (actionCompleted)
			{
				_completedActions.Remove(action);	
			}
			
			EmitSignal(SignalName.ActionReleased, action, actionCompleted);
		}
	}
	
	public void ClearActionHold(StringName action)
	{
		if (_actionsAwaitingHoldTimers.ContainsKey(action))
		{
			var t = _actionsAwaitingHoldTimers[action];
			t.Stop();
			t.QueueFree();
			_actionsAwaitingHoldTimers.Remove(action);
		}
		
		if (_heldActionTimers.ContainsKey(action))
		{
			var t = _heldActionTimers[action];
			t.Stop();
			t.QueueFree();
			_heldActionTimers.Remove(action);
		}

		if (_completedActions.Contains(action))
		{
			_completedActions.Remove(action);	
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