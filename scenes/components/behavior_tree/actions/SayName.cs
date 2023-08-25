using Godot;
using System.Threading.Tasks;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class SayName : Action
{
	protected override Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		GD.Print("Hello!");
		Succeed();
		return Task.FromResult(Status);
	}
}
