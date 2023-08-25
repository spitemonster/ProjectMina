using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class MoveToTargetPosition : Action
{
	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		character.NavigationAgent.TargetPosition = (Vector3)blackboard.GetValue("target_position");

		if (!character.NavigationAgent.IsTargetReachable())
		{
			Fail();
			return Status;
		}

		await ToSignal(character.NavigationAgent, NavigationAgent3D.SignalName.TargetReached);
		GD.Print("reached destination");
		Succeed();
		return Status;
	}
}
