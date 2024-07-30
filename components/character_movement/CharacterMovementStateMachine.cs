using System;
using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class CharacterMovementStateMachine : StateMachine
{

	private CharacterBase _character;
	private DevMonitor _devMonitor;
	private DevMonitor _previousDevMonitor;
	
	public bool CharacterSprintJumped = false;

	public Vector3 GetCharacterVelocity(Vector2 movementInput, double delta)
	{
		StringName nextState = GetTransition();
		
		if (!String.IsNullOrEmpty(nextState))
		{
			RequestTransition(nextState);
		}
		
		var currentState = (CharacterMovementState)GetCurrentState();
		_devMonitor?.SetValue(CurrentState);
		_previousDevMonitor?.SetValue(PreviousState);
		
		return currentState.TickMovement(movementInput, delta);
	}

	public override StringName GetTransition()
	{
		switch (CurrentState)
		{
			case "Sprint":
			case "Walk":
				if (_character.MovementInput.Length()== 0.0)
				{
					return "Idle";
				}
            	break;
			case "Idle":
				if (_character.MovementInput.Length() > 0.0)
				{
					return "Walk";
				}
            	break;
		}
		
		return null;
	}
	public override void _Ready()
	{
		base._Ready();
		
		_character = GetOwner<CharacterBase>();
		
		if (_character == null)
		{
			GD.PushError("Character Movement State Machine: ", Name, " is not child of Character Base.");
			return;
		}

		foreach (var state in States.Values)
		{
			if (state is CharacterMovementState m)
			{
				m.Setup(_character);
			}
		}

		CallDeferred("_InitDevMonitor");
		Start();
	}

	private void _InitDevMonitor()
	{
		_devMonitor = Dev.UI.AddDevMonitor("Movement State: ", Colors.Purple, "Player:Movement");
		_previousDevMonitor = Dev.UI.AddDevMonitor("Previous Movement State: ", Colors.Purple, "Player:Movement");
	}

	public bool CanSprint()
	{
		return false;
		// return States.ContainsKey("Sprint") && PlayerInput.GetMovementInputStrength() > 0.0 && !context.Falling;
	}
}
