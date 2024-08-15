using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EQS;

public partial class SelectRandom : Test
{
	[Export] public int FromTop = 10;

	private RandomNumberGenerator _rng = new();
	// Called when the node enters the scene tree for the first time.
	public override async Task<Array<QueryPoint>> RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
	{
		return await Task.Run(() =>
		{
			if (queryPoints.Count < 1)
			{
				return queryPoints;
			}
			
			_rng.Randomize();
			
			var points = new Array<QueryPoint>(queryPoints.OrderByDescending(point => point.TotalScore / point.TestCount));

			int rand = _rng.RandiRange(0, FromTop);

			if (TestType is EQueryTestType.Filter or EQueryTestType.Both)
			{
				int i = 0;
				
				foreach (var queryPoint in points)
				{
					if (i == rand)
					{
						queryPoint.AddScore(queryPoint.TestCount + 1);
					}
					else
					{
						queryPoint.AddScore(-queryPoint.TestCount - 1);
					}
					i++;
				}

				return points;
			}

			var randomPoint = points[rand];

			randomPoint.AddScore(randomPoint.TestCount + 1);

			return points;
		});
	}
}
