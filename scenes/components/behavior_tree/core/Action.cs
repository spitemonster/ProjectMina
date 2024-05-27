using System.Threading.Tasks;
using Godot;
using Godot.Collections;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public abstract partial class Action : Node
{
	[Signal] public delegate void ActionCompletedEventHandler(ActionStatus ActionStatus);
	[Signal] public delegate void ActionStatusChangedEventHandler(ActionStatus newStatus);
	[Export] public bool IsActive { get; private set; } = true;
	// if an action that inherits this class should modify the blackboard directly, such as finding and setting a target location, it can be set here
	[Export] public StringName BlackboardKey { get; protected set; }
	public ActionStatus Status { get; private set; }
	[Export] protected bool _debug;
	protected Array<Action> _childActions = new();

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		
		foreach (Node c in GetChildren())
		{
			if (c is Action a)
			{
				_childActions.Add(a);
			}
		}

		if (IsActive)
		{
			Succeed();
		}
		else
		{
			Fail();
		}
	}

	public async Task<ActionStatus> Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		if (!IsActive)
		{
			Fail();
			return ActionStatus.Failed;
		}

		if (Status == ActionStatus.Running)
		{
			return ActionStatus.Running;
		}

		Task<ActionStatus> tickAction = _Tick(agent, blackboard);

		if (!tickAction.IsCompleted)
		{
			await tickAction;
		}

		EmitSignal(SignalName.ActionCompleted, (int)tickAction.Result);

		return tickAction.Result;
	}

	// override this function 
	protected abstract Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard);

	protected void SetStatus(ActionStatus newStatus)
	{
		Status = newStatus;

		EmitSignal(SignalName.ActionStatusChanged, (int)Status);
	}

	protected void Succeed()
	{
		SetStatus(ActionStatus.Succeeded);
	}

	protected void Fail()
	{
		SetStatus(ActionStatus.Failed);
	}

	protected void Run()
	{
		SetStatus(ActionStatus.Running);
	}

	private ActionStatus _Execute()
	{
		return ActionStatus.Succeeded;
	}
}
