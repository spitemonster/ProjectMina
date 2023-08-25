using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass, Icon("res://scenes/components/behavior_tree/core/icons/chart-icon.svg")]
public partial class BehaviorTreeComponent : Node
{

	public bool Started { get; private set; } = false;

	[Export] public bool IsActive { get; private set; } = true;
	[Export] private BlackboardComponent _blackboard;

	private Action _root;
	private int _loopCount;
	private AICharacter _character;

	public override void _Ready()
	{
		SetProcess(false);
		_character = GetOwner<CharacterBody3D>() as AICharacter;
		_root = GetChild<Action>(0);

		System.Diagnostics.Debug.Assert(GetChildCount() == 1, "A Behavior Tree can only have one entry point.");
		SetPhysicsProcess(false);
	}

	public void Start()
	{
		if (!Started)
		{
			SetProcess(true);
			Started = true;
		}
	}

	public override async void _Process(double delta)
	{
		if (!IsActive || _root == null || _character == null || _blackboard == null)
		{
			SetProcess(false);
			return;
		}

		Task tickAction = _root.Tick(_character, _blackboard);

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

		// removing because this returns false because AI character is not a tool. I think I'm smart enough not to do this; I also could use behavior trees elsewhere so...
		// if (GetOwner<AICharacter>() is null)
		// {
		// 	warnings.Add("A Behavior Tree must be a direct descendent of an AI Character.");
		// }

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
