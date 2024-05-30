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
	
	public static void SetMouseCapture(bool shouldCapture)
	{
		Input.MouseMode = shouldCapture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
	}

	public static float GetMovementInputStrength()
	{
		return GetInputDirection().Length();
	}

	public static Vector2 GetInputDirection()
	{
		var inputDirection = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_back");

		return inputDirection;
	}
	
	/// <summary>
	/// removes an action's hold timer if one exists
	/// ideally i'd like this to return whether or not it successfully cleared but too many layers to be ale to do consistently
	/// </summary>
	/// <param name="action">name of the action we're clearing</param>
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
		//core input setup
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
	
	/// <summary>
	/// starts the pre-hold timer for a given action
	/// </summary>
	/// <param name="action">the action we're starting the pre-hold timer</param>
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
	
	/// <summary>
	/// when the pre-hold timer times out
	/// remove the pre-hold timer, start the hold timer
	/// </summary>
	/// <param name="action">action name</param>
	/// <param name="waitTimer">pre-hold timer</param>
	private void HoldWaitTimeout(StringName action, Timer waitTimer)
	{
		if (_actionsAwaitingHoldTimers.ContainsKey(action))
		{
			_actionsAwaitingHoldTimers.Remove(action);
			HandleActionHoldStart(action);
		}
		
		waitTimer.QueueFree();
	}
	
	/// <summary>
	/// start the hold timer for a given action
	/// </summary>
	/// <param name="action">action name</param>
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
	
	/// <summary>
	/// cancel a given hold and emit a signal with the action name and completion ratio
	/// this is typically done by releasing the action before the hold is completed
	/// </summary>
	/// <param name="action"></param>
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
	
	/// <summary>
	/// hold timer completed successfully.
	/// </summary>
	/// <param name="action">action name</param>
	/// <param name="holdTimer">hold timer</param>
	private void HoldTimeout(StringName action, Timer holdTimer)
	{
		if (_heldActionTimers.ContainsKey(action))
		{
			HandleActionHoldComplete(action);
		}
			
		holdTimer.QueueFree();
	}

	/// <summary>
	/// action hold completed logic
	/// </summary>
	/// <param name="action"></param>
	private void HandleActionHoldComplete(StringName action)
	{
		_heldActionTimers.Remove(action);
		_completedActions.Add(action);
		EmitSignal(SignalName.ActionHoldCompleted, action);
	}

	/// <summary>
	/// deals with hold release
	/// </summary>
	/// <param name="action"></param>
	private void HandleActionRelease(StringName action)
	{
		// if the action was in the pre-hold stage, execute a press
		if (_actionsAwaitingHoldTimers.ContainsKey(action))
		{
			var t = _actionsAwaitingHoldTimers[action];
			t.Stop();
			t.QueueFree();
			
			_actionsAwaitingHoldTimers.Remove(action);
			EmitSignal(SignalName.ActionPressed, action);
		} else if (_heldActionTimers.ContainsKey(action))
		{
			// otherwise if a full hold has started, cancel the hold
			EmitSignal(SignalName.ActionReleased, action, false);
			HandleActionHoldCancel(action);
		} else
		{
			// otherwise just release the action and clear it if the action was completed, which it should be in most cases
			EmitSignal(SignalName.ActionReleased, action, _completedActions.Contains(action));
			if (_completedActions.Contains(action))
			{
				_completedActions.Remove(action);	
			}
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
}
