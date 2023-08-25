using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class Wait : Action
{
	[Export] public float WaitTime;

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		await ToSignal(GetTree().CreateTimer(WaitTime), "timeout");
		Succeed();
		return Status;
	}

}
