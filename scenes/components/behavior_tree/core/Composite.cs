using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Composite : Action
{
	public override void _Ready()
	{

	}

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		Task<ActionStatus> tickAction = null;

		foreach (Action task in GetChildren())
		{
			if (task != null)
			{
				tickAction = task.Tick(character, blackboard);

				if (!tickAction.IsCompleted)
				{
					await tickAction;
				}
			}
		}

		Succeed();
		return Status;
	}
}