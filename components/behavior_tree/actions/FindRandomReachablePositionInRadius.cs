using Godot;
using System.Threading.Tasks;
using Godot.Collections;

namespace ProjectMina.BehaviorTree;

public partial class FindRandomReachablePositionInRadius : Action
{
	[Export] public float Radius { get; protected set; } = 10.0f;
	[Export] public float MaxPathDistance { get; protected set; } = 8.0f;
	[Export] public int MaxIterations = 4;

	private int _currentIteration = 1;

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
		bool positionFound = false;
		var characterPosition = controller.Pawn.GlobalPosition;
		var newPosition = characterPosition;
		Vector3 closestPoint = Vector3.Zero;

		while (!positionFound)
		{
			if (_currentIteration >= MaxIterations)
			{
				break;
			}
			var x = characterPosition.X + rng.RandfRange(-12.5f, 12.5f);
			var z = characterPosition.Z + rng.RandfRange(-12.5f, 12.5f);

			newPosition.X = x;
			newPosition.Z = z;
			newPosition.Y = characterPosition.Y;

			var targetPoint = NavigationServer3D.MapGetClosestPoint(controller.NavigationAgent.GetNavigationMap(), newPosition);
			Vector3[] path = NavigationServer3D.MapGetPath(controller.NavigationAgent.GetNavigationMap(),
				characterPosition, targetPoint, false);

			var pathLength = characterPosition.DistanceTo(path[0]);

			for (var i = 1; i < path.Length; i++)
			{
				var pos = path[i];
				var dist = pos.DistanceTo(path[i - 1]);
				pathLength += dist;
			}

			if (pathLength <= MaxPathDistance)
			{
				closestPoint = targetPoint;
				positionFound = true;
			}

			_currentIteration++;
		}

		DebugDraw.Sphere(closestPoint, 1, Colors.Bisque, 10f);
		_currentIteration = 0;
		return closestPoint;
		
		
		
	}

}
