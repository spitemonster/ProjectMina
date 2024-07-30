using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass,Icon("res://_dev/icons/icon--selector.svg")]
public partial class Selector : Composite
{
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		GD.Print("child count: ", GetChildCount());
		
		foreach (Action child in GetChildren())
		{
			if (child == null)
			{
				continue;
			}

			EActionStatus status = child.Tick(controller, blackboard);
			
			
			GD.Print("Ticking Action: ", child.Name, ". Status: ", status);

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