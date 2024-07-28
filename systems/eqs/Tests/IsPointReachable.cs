using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;
using Godot.Collections;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class IsPointReachable : Test
{

	private Array<QueryPoint> _safePoints = new();
	protected override float TestPoint(QuerierInfo querierInfo, QueryPoint queryPoint)
	{
		return 1.0f;
	}

	public override async Task<Array<QueryPoint>> RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
	{
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		// CallDeferred("_RunTest", controller, queryPoints);
		return _RunTest(controller, queryPoints);
	}

	private Array<QueryPoint> _RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
	{
		if (Engine.IsEditorHint())
		{
			return null;
		}
		
		var points = queryPoints;
		var map = controller.NavigationAgent.GetNavigationMap();
		var maxDist = controller.NavigationAgent.TargetDesiredDistance;
		var origin = controller.Pawn.GlobalPosition;
		var spaceState = controller.Pawn.GetWorld3D().DirectSpaceState;
		
		foreach (var queryPoint in points)
		{
			var pointNearestPosition = NavigationServer3D.MapGetClosestPoint(map, queryPoint.GlobalPosition);
			var dist = pointNearestPosition.DistanceTo(queryPoint.GlobalPosition);

			// if the nearest point position is far enough away from the actual point, the point probably isn't on the nav mesh so we can probably discard it as an option
			if (dist > maxDist)
			{
				if (EnableDebug)
				{
					DebugDraw.Sphere(pointNearestPosition, 1.125f, Colors.Orange, 4f);
					GD.PushError("Point at position: ", queryPoint.GlobalPosition, " appears to be off the navigation mesh.");
				} 
				queryPoint.AddScore(0);
				continue;
			}
			
			var path = NavigationServer3D.MapGetPath(map, origin, pointNearestPosition, false);
			var lastPoint = path[^1];
			
			// if we can't reach the point, don't bother with it 
			if (path.Length < 1)
			{
				if (EnableDebug)
				{
					DebugDraw.Sphere(pointNearestPosition, 1.25f, Colors.Pink, 5f);
					GD.PushError("No viable path to position near point at position: ", queryPoint.GlobalPosition);
				}

				queryPoint.AddScore(0);
				continue;
			}

			PhysicsShapeQueryParameters3D traceQuery = new()
			{
				CollideWithAreas = true,
				CollideWithBodies = true,
				Exclude = new() { controller.Pawn.GetRid() },
				Shape = new SphereShape3D()
				{
					Radius = 1f,
				},
				Transform = new()
				{
					Origin = queryPoint.GlobalPosition + new Vector3(0, 1f, 0),
					Basis = Basis.Identity
				}
			};
			
			// DebugDraw.Sphere(traceQuery.Transform, Colors.Red);
			
			var res = spaceState.CollideShape(traceQuery, 1);
			
			// if (EnableDebug)
			// {
			// 	DebugDraw.Sphere(traceQuery.Transform, 1.25f, Colors.Pink, 5f);
			// 	GD.PushError("No viable path to position near point at position: ", queryPoint.GlobalPosition);
			// }
			
			// if the point isn't reachable or there's a collision in the point
			if (res.Count == 0)
			{
				queryPoint.AddScore(1);
			}
			else if (TestType is EQueryTestType.Both or EQueryTestType.Filter)
			{
				queryPoint.AddScore(-queryPoint.TestCount - 1);
			}
		}

		return points;
	}
}
