using Godot;
using Godot.Collections;

namespace ProjectMina;
[GlobalClass]
public partial class State : Node
{
	[Signal] public delegate void TransitionEventHandler(StringName newStateName);

	[Export] public bool AnyPreviousState = true;
	[Export] public Array<StringName> AllowedPreviousStates = new();
	
	[Export] public bool AnyNextState = true;
	[Export] public Array<StringName> AllowedNextStates = new();
	

	protected StateMachine Machine;

	public override void _Ready()
	{
		Machine = GetParent<StateMachine>();
	}
	public virtual void EnterState(StringName previousState) {}

	public virtual void ExitState(StringName nextState) {}

	public virtual void Tick(double delta) {}

	public virtual void PhysicsTick(double delta) {}

	public virtual bool CanExitTo(StringName state)
	{
		return AllowedNextStates.Contains(state) || AnyNextState;
	}

	public virtual bool CanEnterFrom(StringName state)
	{
		return AllowedPreviousStates.Contains(state) || AnyPreviousState;
	}
}
