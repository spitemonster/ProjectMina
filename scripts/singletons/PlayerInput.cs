using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class PlayerInput : Node
{
	[Signal] public delegate void MouseMoveEventHandler(Vector2 mouseDelta);
	[Signal] public delegate void ActionPressedEventHandler(StringName action);
	[Signal] public delegate void ActionReleasedEventHandler(StringName action, bool actionCompleted);
	[Signal] public delegate void ActionHoldStartedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCompletedEventHandler(StringName action);
	[Signal] public delegate void ActionHoldCanceledEventHandler(StringName action, float completedRatio);
	[Signal] public delegate void PausedEventHandler();
	[Signal] public delegate void UnpausedEventHandler();
	
	public static PlayerInput Manager { get; private set; }

	private InputManagerSettings _settings;
	
	private Array<StringName> _completedActions = new();
	private Dictionary<StringName, Timer> _actionsAwaitingHoldTimers = new();
	private Dictionary<StringName, Timer> _heldActionTimers = new();

	private Array<StringName> _actions;
	
	public void SetPause(bool shouldPause)
	{
		GetTree().Paused = shouldPause;

		EmitSignal(shouldPause ? SignalName.Paused : SignalName.Unpaused);
	}
	
	public void SetMouseCapture(bool shouldCapture)
	{
		Input.MouseMode = shouldCapture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
	}

	public static Vector2 GetInputDirection()
	{
		Vector2 inputDirection = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_back");

		return inputDirection;
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
	
	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion mouseMove)
		{
			EmitSignal(SignalName.MouseMove, mouseMove.Relative);
		}
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

	public override void _Ready()
	{
		_settings = ResourceLoader.Load<InputManagerSettings>("res://resources/settings/InputSettings.tres");
		_actions = InputMap.GetActions();

		if (_settings == null || _actions.Count < 1)
		{
			GD.PushError("Input settings or input actions are missing");
		}
	}
	
	public override void _EnterTree()
	{
		if (Manager == null)
		{
			Manager = this;
			return;
		}

		QueueFree();
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
	
	private void HoldTimeout(StringName action, Timer holdTimer)
	{
		if (_heldActionTimers.ContainsKey(action))
		{
			HandleActionHoldComplete(action);
		}
			
		holdTimer.QueueFree();
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
}
