using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Selector : Composite
{
	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}
			
			GD.Print("blackboard selector checking child: ", child.Name);

			ActionStatus status = await child.Tick(agent, blackboard);

			if (status == ActionStatus.Succeeded)
			{
				GD.Print("blackboard selector child succeeded");
				Succeed();
				return Status;
			}
		}

		Fail();
		return Status;
	}
}