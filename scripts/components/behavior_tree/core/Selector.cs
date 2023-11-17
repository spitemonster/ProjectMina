using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Selector : Composite
{
	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}

			Task<ActionStatus> tickAction = child.Tick(character, blackboard);

			if (!tickAction.IsCompleted)
			{
				await tickAction;
			}

			if (child.Status == ActionStatus.SUCCEEDED)
			{
				GD.Print("child ticked!");
				Succeed();
				return Status;
			}
		}

		Fail();
		return Status;
	}
}