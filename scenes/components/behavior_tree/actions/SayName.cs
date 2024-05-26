using Godot;
using System.Threading.Tasks;
namespace ProjectMina.BehaviorTree;

public partial class SayName : Action
{
	protected override Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		GD.Print("Saying name! ", agent.Pawn.Name);
		Succeed();
		return Task.FromResult(Status);
	}
}
