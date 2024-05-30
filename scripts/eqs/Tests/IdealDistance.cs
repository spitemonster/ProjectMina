using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EnvironmentQuerySystem;

public partial class IdealDistance : Test
{
	[Export] public float MinDistance = 0.0f;
	[Export] public float MaxDistance = 10.0f;

	public float MinDistanceSquared => MinDistance * MinDistance;
	public float MaxDistanceSquared => MaxDistance * MaxDistance;

	private Array<Vector3> _safePoints = new();
	// Called when the node enters the scene tree for the first time.
	public override async Task<Array<Vector3>> RunTest(AgentComponent agent, Array<Vector3> items)
	{
		CallDeferred("_RunTest", agent, items);
		await ToSignal(this, SignalName.TestCompleted);
		return _safePoints;
	}

	private void _RunTest(AgentComponent agent, Array<Vector3> items)
	{
		Vector3 origin = agent.Pawn.GlobalPosition;
		
		Array<Vector3> safePoints = new();

		foreach (var point in items)
		{
			var dist = point.DistanceSquaredTo(origin);
			if (dist >= MinDistanceSquared && dist <= MaxDistanceSquared)
			{
				DebugDraw.Sphere(point, .85f, Colors.Cyan, 5.0f);
				safePoints.Add(point);
			}	
		}

		_safePoints = safePoints;

		EmitSignal(SignalName.TestCompleted, _safePoints);
	}

	protected override float CalculateScore(Vector3 point)
	{
		return 0.0f;
	}
}
