using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Wait : Action
{
	[Export] public float WaitTime;

	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		await ToSignal(GetTree().CreateTimer(WaitTime), "timeout");
		Succeed();
		return Status;
	}

}
