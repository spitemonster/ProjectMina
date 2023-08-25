using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class Selector : Composite
{

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		GD.Print("selector ticking");
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}

			GD.Print("ticking child: " + child.Name);

			Task<ActionStatus> tickAction = child.Tick(character, blackboard);

			if (!tickAction.IsCompleted)
			{
				await tickAction;
			}

			if (child.Status == ActionStatus.SUCCEEDED)
			{
				Succeed();
				return Status;
			}
		}

		Fail();
		return Status;
	}
}