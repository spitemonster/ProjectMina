using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public enum ActionStatus: int
{
	Succeeded,
	Failed,
	Running
}

[Tool]
[GlobalClass]
public partial class BehaviorTreeComponent : ComponentBase
{

	public bool Started { get; private set; } = false;

	[Export] public bool IsActive { get; private set; } = true;
	[Export] private BlackboardComponent _blackboard;

	private Action _root;
	private int _loopCount;
	private AgentComponent _agent;

	public override void _Ready()
	{
		base._Ready();
		_root = GetChild<Action>(0);

		System.Diagnostics.Debug.Assert(GetChildCount() == 1, "A Behavior Tree can only have one entry point.");
		SetPhysicsProcess(false);
	}

	public void Start(AgentComponent agent)
	{
		if (!Started)
		{
			GD.Print("starting!");
			
			Started = true;
			SetProcess(true);
			_agent = agent;
		}
	}

	public override async void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		
		if (!Started)
		{
			GD.Print("not started");
			return;
		}
		
		GD.Print("fuck");
		
		
		base._Process(delta);

		Task<ActionStatus> tickAction = _root.Tick(_agent, _blackboard);

		if (!tickAction.IsCompleted)
		{
			SetProcess(false);

			await tickAction;

			SetProcess(true);
			_loopCount++;
		}
	}

	private void Abort()
	{
		IsActive = false;
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
