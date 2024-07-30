using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class MoveToTargetPosition : Action
{
	private bool _navigationStarted = false;
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		GD.Print("target pos: ", controller.NavigationAgent.TargetPosition);

		var targetPosition = blackboard.GetValueAsVector3(BlackboardKey);
		var navTarget = controller.NavigationAgent.TargetPosition;
		if (!_navigationStarted || (_navigationStarted && navTarget != targetPosition))
		{
			_navigationStarted = true;
			controller.SetTargetPosition(targetPosition);
		}
		
		if (controller.NavigationAgent.IsTargetReached())
		{
			_navigationStarted = false;
			GD.Print("should finish successfully");
			return EActionStatus.Succeeded;	
		}

		return EActionStatus.Running;
	}
}
