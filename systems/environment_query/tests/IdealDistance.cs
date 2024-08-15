using System.Collections.Generic;
using System.Drawing;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EQS;

public partial class IdealDistance : Test
{
	[Export] public float MinDistance = 0.0f;
	[Export] public float MaxDistance = 10.0f;

	public float MinDistanceSquared => MinDistance * MinDistance;
	public float MaxDistanceSquared => MaxDistance * MaxDistance;

	private Array<QueryPoint> _safePoints = new();
	// Called when the node enters the scene tree for the first time.
	public override async Task<Array<QueryPoint>> RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
	{
		return await base.RunTest(controller, queryPoints);
	}
	
	protected override float TestPoint(QuerierInfo querierInfo, QueryPoint queryPoint)
	{
		var origin = querierInfo.QuerierPosition;
		var dist = queryPoint.GlobalPosition.DistanceSquaredTo(origin);
		var score = 0.0f;
		
		if (dist >= MinDistanceSquared)
		{
			score += .3f;
		}

		if (dist <= MaxDistanceSquared)
		{
			score += .3f;
		}

		if (dist >= MaxDistanceSquared && dist <= MaxDistanceSquared)
		{
			score += .4f;
		}
		
		return score;
	}
}
