using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class LookAt : Action
{
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		GD.Print("should look at");
		Vector3 dir = new();
			
		if (blackboard.GetValueAsObject(BlackboardKey) is Node3D n )
		{
			dir = controller.Pawn.GlobalPosition.DirectionTo(n.GlobalPosition).Normalized();
				
		}
			
		var controlInput = new Vector2
		{
			X = dir.X,
			Y = dir.Z
		};
			
		controller.Pawn.SetControlInput(controlInput);

		SetStatus(EActionStatus.Running);
		return Status;
	}
}
