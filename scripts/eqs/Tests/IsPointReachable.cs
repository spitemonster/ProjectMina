using System.Linq;
using Godot;
using System.Threading.Tasks;
using Godot.Collections;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class IsPointReachable : Test
{

	private Array<Vector3> _safePoints = new();
	protected override float CalculateScore(Vector3 point)
	{
		return 1.0f;
	}

	public override async Task<Array<Vector3>> RunTest(AgentComponent agent, Array<Vector3> items)
	{
		CallDeferred("_TestPointsReachable", agent, items);

		await ToSignal(this, SignalName.TestCompleted);
		
		Dictionary<Vector3, float> scoreDict = new();

		foreach (var item in _safePoints)
		{
			var score = CalculateScore(item);

			// immediately filter out anything below the threshold if we should be filtering
			if (TestType is EQueryTestType.Filter or EQueryTestType.Both)
			{
				if (score > MinScore && score <= MaxScore)
				{
					scoreDict.Add(item, score);
				}
			}
			else
			{
				scoreDict.Add(item, score);
			}
		}

		// return an empty array if all items were removed
		if (scoreDict.Count < 1)
		{
			return new Array<Vector3>();
		}

		// if we're supposed to sort the array, do that
		if (TestType is EQueryTestType.Score or EQueryTestType.Both)
		{
			// absolutely roundabout way to sort a dictionary and then convert it back to a dictionary
			// orderbydescending returns a generic c# enumerable so we need to cast it to a system dict from which we can make a new Godot dict
			System.Collections.Generic.Dictionary<Vector3, float> test =
				new(scoreDict.OrderByDescending(pair => pair.Value));

			scoreDict = new Godot.Collections.Dictionary<Vector3, float>(test);
		}

		return (Array<Vector3>)scoreDict.Keys;
	}

	private void _TestPointsReachable(AgentComponent agent, Array<Vector3> items)
	{
		var map = agent.Pawn.NavigationAgent.GetNavigationMap();
		var maxDist = agent.Pawn.NavigationAgent.TargetDesiredDistance;
		var origin = agent.Pawn.GlobalPosition;
		var world = agent.Pawn.GetWorld3D();
		var spaceState = world.DirectSpaceState;

		Array<Vector3> safePoints = new();
		
		foreach (var point in items)
		{
			var path = NavigationServer3D.MapGetPath(map, origin, point, false);
			var lastPoint = path[^1];
			HitResult res = Cast.Sphere(spaceState, point, 1, new());
			
			if (res != null && res.Collider != null)
			{
				DebugDraw.Sphere(point, .75f, Colors.Red, 5f);
			}

			if (lastPoint.DistanceTo(point) > maxDist)
			{
				DebugDraw.Sphere(point, .66f, Colors.Orange, 5f);
			}

			if (lastPoint.DistanceTo(point) <= maxDist && (res == null || res.Collider == null))
			{
				DebugDraw.Sphere(point, .7f, Colors.Blue, 5f);
				safePoints.Add(point);
			}
		}

		_safePoints = safePoints;
		EmitSignal(SignalName.TestCompleted, _safePoints);
	}
}
