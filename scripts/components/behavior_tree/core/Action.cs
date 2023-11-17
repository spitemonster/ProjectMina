using System.Threading.Tasks;
using Godot;
using Godot.Collections;
namespace ProjectMina.BehaviorTree;

public abstract partial class Action : Node
{
	public enum ActionStatus
	{
		SUCCEEDED,
		FAILED,
		RUNNING
	}

	[Signal] public delegate void ActionCompletedEventHandler(ActionStatus ActionStatus);
	[Signal] public delegate void ActionStatusChangedEventHandler(ActionStatus newStatus);

	protected Array<Action> _childActions = new();

	public ActionStatus Status { get; private set; }

	[Export] public bool IsActive { get; private set; } = true;

	[Export] protected bool _debug;

	public override void _Ready()
	{
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

	public async Task<ActionStatus> Tick(AICharacter character, BlackboardComponent blackboard)
	{
		if (!IsActive)
		{
			Fail();
			return ActionStatus.FAILED;
		}

		if (Status == ActionStatus.RUNNING)
		{
			return ActionStatus.RUNNING;
		}

		Task<ActionStatus> tickAction = _Tick(character, blackboard);

		if (!tickAction.IsCompleted)
		{
			await tickAction;
		}

		EmitSignal(SignalName.ActionCompleted, (int)tickAction.Result);

		return tickAction.Result;
	}

	// override this function 
	protected abstract Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard);

	protected void SetStatus(ActionStatus newStatus)
	{
		Status = newStatus;

		EmitSignal(SignalName.ActionStatusChanged, (int)Status);
	}

	protected void Succeed()
	{
		SetStatus(ActionStatus.SUCCEEDED);
	}

	protected void Fail()
	{
		SetStatus(ActionStatus.FAILED);
	}

	protected void Run()
	{
		SetStatus(ActionStatus.RUNNING);
	}

	private ActionStatus _Execute()
	{
		return ActionStatus.SUCCEEDED;
	}
}
