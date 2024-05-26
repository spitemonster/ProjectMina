using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class Sequence : Composite
{
	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}

			Task<ActionStatus> tickAction = child.Tick(agent, blackboard);

			if (!tickAction.IsCompleted)
			{
				await tickAction;
			}

			if (child.Status == ActionStatus.Failed)
			{
				Fail();
				return Status;
			}
		}

		Succeed();

		return Status;
	}
}
