// condition nodes are intended to run code that tests if its child should run. typically this is used for blackboard lookups
using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class Condition : Action
{
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		SetStatus(EActionStatus.Succeeded);
		return Status;
	}

	public override string[] _GetConfigurationWarnings()
	{
		Godot.Collections.Array<string> warnings = new();

		if (GetChildCount() != 1)
		{
			warnings.Add("Conditional Nodes are expected to have precisely child.");
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
