using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class MoveToTargetPosition : Action
{
	private bool _navigationStarted = false;
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		var targetPosition = blackboard.GetValueAsVector3(BlackboardKey);
		var navTarget = controller.NavigationAgent.TargetPosition;
		
		if (!_navigationStarted || (_navigationStarted && navTarget != targetPosition && targetPosition != Vector3.Zero))
		{
			_navigationStarted = true;
			controller.SetTargetPosition(targetPosition);
		}
		
		if (controller.NavigationAgent.IsTargetReached())
		{
			Dev.UI.PushDevNotification("target reached");
			_navigationStarted = false;
			return EActionStatus.Succeeded;	
		}

		return EActionStatus.Running;
	}
}
