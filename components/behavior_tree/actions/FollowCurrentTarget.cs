using Godot;
using System.IO;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class FollowCurrentTarget : Action
{
	[Export] public string TargetKey;

	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		var currentFocus = (Node3D)blackboard.GetValueAsObject(TargetKey);
		Vector3 targetPosition = currentFocus.GlobalPosition;
		
		if (targetPosition != Vector3.Zero) {
			blackboard.SetValueAsVector3(BlackboardKey, targetPosition);
			SetStatus(EActionStatus.Succeeded);
		}
		else
		{
			SetStatus(EActionStatus.Running);
		}

		return Status;
	}
}
 