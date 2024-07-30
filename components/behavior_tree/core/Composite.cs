using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Composite : Action
{

	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{

		SetStatus(EActionStatus.Succeeded);
		return Status;
	}
}