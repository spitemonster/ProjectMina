using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass,Icon("res://resources/images/icons/icon--selector.svg")]
public partial class Selector : Composite
{
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}

			EActionStatus status = child.Tick(controller, blackboard);
			if (status == EActionStatus.Succeeded)
			{
				SetStatus(EActionStatus.Succeeded);
				return Status;
			}
		}

		SetStatus(EActionStatus.Failed);
		return Status;
	}
}