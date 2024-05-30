using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EnvironmentQuerySystem;

public partial class SelectRandom : Test
{
	// Called when the node enters the scene tree for the first time.
	public override async Task<Array<Vector3>> RunTest(AgentComponent agent, Array<Vector3> items)
	{
		return await Task.Run(() =>
		{
			var random = items.PickRandom();
			DebugDraw.Sphere(random, 1.3f, Colors.Chocolate, 7.0f);
			return new Array<Vector3>() { random };
		});
	}
}
