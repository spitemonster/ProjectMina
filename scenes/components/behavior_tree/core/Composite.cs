using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Composite : Action
{
	public override void _Ready()
	{

	}

	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		Task<ActionStatus> tickAction = null;

		foreach (Action task in GetChildren())
		{
			if (task == null)
			{
				continue;
			}
			
			tickAction = task.Tick(agent, blackboard);

			if (!tickAction.IsCompleted)
			{
				await tickAction;
			}
		}

		Succeed();
		return Status;
	}
}