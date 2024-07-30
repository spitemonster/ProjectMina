using Godot;
using System.Threading.Tasks;
namespace ProjectMina.BehaviorTree;

public partial class SayName : Action
{
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		Dev.UI.PushDevNotification("Hello! My name is: " + controller.Pawn.Name);
		SetStatus(EActionStatus.Succeeded);
		return Status;
	}
}
