using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
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

	public ActionStatus Status { get; private set; }

	[Export] public bool IsActive { get; private set; } = true;

	public override void _Ready()
	{
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
	protected virtual async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		return await Task.Run(() =>
		{
			return ActionStatus.SUCCEEDED;
		});
	}

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

	public override string[] _GetConfigurationWarnings()
	{
		Godot.Collections.Array<string> warnings = new();

		foreach (var child in GetChildren())
		{

			if (!(child is Action))
			{
				warnings.Add(child.Name + " was expected to be of or inherit type Action.");
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
}
