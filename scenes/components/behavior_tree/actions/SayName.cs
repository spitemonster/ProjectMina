using Godot;
using System.Threading.Tasks;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class SayName : Action
{
	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		GD.Print("Hello!");
		Succeed();
		return Status;
	}
}
