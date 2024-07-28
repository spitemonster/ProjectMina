using Godot;
using System.Threading.Tasks;
using Godot.Collections;

namespace ProjectMina.EnvironmentQuerySystem;

public partial class Debug : Test
{
	public override async Task<Array<QueryPoint>> RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
	{
		return await Task.Run(() =>
		{
			return queryPoints;
		});
	}
}
