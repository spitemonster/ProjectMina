using Godot;
using System;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class PrintDevMessage : Action
{
	[Export] public string DevMessage = "Message";
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		Dev.UI.PushDevNotification(DevMessage);
		SetStatus(EActionStatus.Succeeded);
		return Status;
	}
}
