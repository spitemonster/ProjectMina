using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class FindRandomTargetPositionInRadius : Action
{
	[Export] public float Radius { get; protected set; } = 10.0f;
	[Export] public float MaxPathDistance { get; protected set; } = 8.0f;

	// TODO: move this to a global
	private RandomNumberGenerator rng = new();

	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		Vector3 newTarget = FindPosition(controller, blackboard);
		
		if (newTarget != Vector3.Zero)
		{
			blackboard.SetValue(BlackboardKey, newTarget);
			SetStatus(EActionStatus.Succeeded);	
		}
		else
		{
			SetStatus(EActionStatus.Failed);
		}
		
		return Status;
	}

	private Vector3 FindPosition(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		
		var characterPosition = controller.Pawn.GlobalPosition;
		var newPosition = characterPosition;

		var x = characterPosition.X + rng.RandfRange(-12.5f, 12.5f);
		var z = characterPosition.Z + rng.RandfRange(-12.5f, 12.5f);

		newPosition.X = x;
		newPosition.Z = z;
		newPosition.Y = characterPosition.Y;

		Vector3 pos = NavigationServer3D.MapGetClosestPoint(controller.NavigationAgent.GetNavigationMap(), newPosition);
			
		DebugDraw.Sphere(pos, 1, Colors.Bisque, 10f);
		GD.Print("find position: ", pos.ToString());

		return pos;
	}

}
