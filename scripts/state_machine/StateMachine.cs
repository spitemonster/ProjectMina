using Godot;
using System;
using System.Linq;
using Godot.Collections;

namespace ProjectMina;
[Tool]
[GlobalClass]
public abstract partial class StateMachine : ComponentBase
{
    [Signal] public delegate void StateTransitionedEventHandler(StringName newState, StringName previousState);
    
    [Export] private StringName _defaultStateName;

    public StringName CurrentState => _currentState?.Name;
    public StringName PreviousState => _previousState?.Name;

    private State _currentState;
    private State _previousState;

    protected Dictionary<StringName, State> States = new();

    public virtual void Start()
    {
        if (!String.IsNullOrEmpty(_defaultStateName))
        {
            TransitionStates(_defaultStateName);
        }
        else if (States.Count > 0)
        {
            TransitionStates(States.Keys.ToArray()[0]);
        }
    }
    
    public void RequestTransition(StringName stateName)
    {
        if (States.ContainsKey(stateName) && _currentState.CanExitTo(stateName))
        {
            TransitionStates(stateName);
        }
    }

    public virtual StringName GetTransition()
    {
        return "";
    }
    
    public void TransitionStates(StringName newStateName)
    {
        var newState = States[newStateName];

        if (newState != null)
        {
            if (newState != _currentState)
            {
                // if the current state isn't null, exit
                _currentState?.ExitState(newState.Name);
            }

            _previousState = _currentState;
            _currentState = newState;
            _currentState.EnterState(_previousState?.Name);
            EmitSignal(SignalName.StateTransitioned, CurrentState, PreviousState);
        }
        else
        {
            GD.PushError("State ", newStateName, " does not exist!");
        }
    }
    
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            if (child is State s)
            {
                States.Add(s.Name, s);
                s.Transition += RequestTransition;
            }
        }
    }

    public virtual void Tick(double delta)
    {
        _currentState.Tick(delta);
    }

    public virtual void PhysicsTick(double delta)
    {
        StringName nextState = GetTransition();

        if (!String.IsNullOrEmpty(nextState))
        {
            RequestTransition(nextState);
        }
        
        _currentState.PhysicsTick(delta);
    }

    public override string[] _GetConfigurationWarnings()
    {
        Array<string> warnings = new();

        foreach (var child in GetChildren())
        {
            if (child is not State)
            {
                warnings.Add("State Machine should only contain State child nodes.");
            }
        }

        string[] baseWarnings = base._GetConfigurationWarnings();
        if (baseWarnings != null && baseWarnings.Length > 0)
        {
            warnings.AddRange(baseWarnings);
        }

        string[] errs = new string[warnings.Count];

        for (int i = 0; i < warnings.Count; i++)
        {
            errs.SetValue(warnings[i], i);
        }

        return errs;
    }

    protected State GetCurrentState()
    {
        return _currentState;
    }
}
