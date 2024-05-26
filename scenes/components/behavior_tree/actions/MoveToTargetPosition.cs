using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class MoveToTargetPosition : Action
{
	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		GD.Print("should ");
		
		Vector3 location = (Vector3)blackboard.GetValue("target_movement_position");
		GD.Print("blackboard value: ", location);
		agent.SetTargetPosition(location);

		if (!agent.Pawn.NavigationAgent.IsTargetReachable())
		{
			GD.Print("unable to reach destination for some reason");
			Fail();
			return Status;
		}
		
		GD.Print("should move to target position");

		await ToSignal(agent.Pawn.NavigationAgent, NavigationAgent3D.SignalName.TargetReached);
		GD.Print("moved to target position");

		Succeed();
		return Status;
	}
}
