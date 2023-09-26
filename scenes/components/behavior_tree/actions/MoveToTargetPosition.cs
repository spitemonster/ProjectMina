using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class MoveToTargetPosition : Action
{
	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		GD.Print("moving to target position");
		character.Brain.NavigationAgent.TargetPosition = (Vector3)blackboard.GetValue("target_position");

		if (!character.Brain.NavigationAgent.IsTargetReachable())
		{
			GD.Print("failed to be able to reach destination for some reason");
			Fail();
			return Status;
		}

		await ToSignal(character.Brain.NavigationAgent, NavigationAgent3D.SignalName.TargetReached);
		GD.Print("moved to target position");

		Succeed();
		return Status;
	}
}
