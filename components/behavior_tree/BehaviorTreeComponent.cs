using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public enum EActionStatus: uint
{
	Failed,
	Running,
	Succeeded
}

[Tool]
[GlobalClass, Icon("res://_dev/icons/icon--chart.svg")]
public partial class BehaviorTreeComponent : ComponentBase
{

	public bool Started { get; private set; } = false;
	
	[Export] private BlackboardComponent _blackboard;

	private Action _rootAction;
	private int _loopCount;
	private AIControllerComponent _aiController;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		
		SetProcess(false);
		SetPhysicsProcess(false);
		
		base._Ready();

		System.Diagnostics.Debug.Assert(GetChildCount() == 1, "A Behavior Tree must have only one entry point. Child node count: " + GetChildCount());
	}

	public bool SetController(AIControllerComponent AIController)
	{
		if (_aiController != null)
		{
			return false;
		}

		_aiController = AIController;
		return true;
	}

	public void Start()
	{
		if (!Active || _aiController == null || Started)
		{
			if (EnableDebug)
			{
				GD.PushError("Can't start Behavior Tree ", Name, ". Active: ", Active, ". _agent: ", _aiController, ". Started: ", Started);
			}
			
			return;
		}
		
		Started = true;
		_rootAction = GetChild<Action>(0);
		_rootAction.SetRootAction(_rootAction);
		_rootAction.SetBehaviorTree(this);
		
		SetProcess(true);
		if (EnableDebug)
		{
			GD.Print("Starting Behavior Tree ", Name, ". _agent: ", _aiController, ". Started: ", Started);
		}
	}

	public void Stop()
	{
		if (!Active || !Started)
		{
			return;
		}
		
		SetProcess(false);
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint() || GetTree().Paused)
		{
			return;
		}
		
		if (!Started)
		{
			return;
		}
		
		base._Process(delta);

		EActionStatus tickAction = _rootAction.Tick(_aiController, _blackboard);
		
		if (EnableDebug)
		{
			GD.Print("Ticking root task: ", _rootAction.Name);
		}

		// if (!tickAction.IsCompleted)
		// {
		// 	SetProcess(false);
		//
		// 	// await tickAction;
		//
		// 	SetProcess(true);
		// 	_loopCount++;
		// }
	}

	private void Abort()
	{
		Active = false;
	}

	public override string[] _GetConfigurationWarnings()
	{
		Godot.Collections.Array<string> warnings = new();

		if (GetChildCount() > 1)
		{
			warnings.Add("A Behavior Tree may have only one root node.");
		}

		if (_blackboard == null)
		{
			warnings.Add("A Behavior Tree must have a Blackboard component assigned.");
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
