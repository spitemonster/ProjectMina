using Godot;

using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class Query : EQSNode
{
	public Context QueryContext;
	public Array<Test> QueryTests = new();
	
	// this should return a variant that the RunEnvironmentQuery action can set as a result of it running.
	public async Task<Vector3> RunQuery(AgentComponent querier)
	{
		if (QueryContext == null || QueryTests == null)
		{

			if (Debug)
			{
				GD.PushError("Can't run query, missing context or tests");
			}

			return Vector3.Zero;
		}
		
		Array<Vector3> queryPoints = await QueryContext.GetPoints(querier);
		
		foreach (var test in QueryTests)
		{
			queryPoints = await test.RunTest(querier, queryPoints);
		}
		
		if (queryPoints.Count < 1)
		{
			DebugDraw.Sphere(Vector3.Zero, .5f, Colors.Black, 5.0f, false);
			return Vector3.Zero;
		} 
		
		DebugDraw.Sphere(queryPoints[0], .75f, Colors.Blue, 5.0f, false);

		return queryPoints[0];
	}
	
	public override string[] _GetConfigurationWarnings()
	{
		Array<string> warnings = new();
		if (GetChildCount() > 1)
		{
			warnings.Add("An EQS Query must only have one child.");
		}

		if (GetChildren()[0] is not Context)
		{
			warnings.Add("An EQS Query child must be a Context node.");
		}

		string[] baseWarnings = base._GetConfigurationWarnings();
		if (baseWarnings != null && baseWarnings.Length > 0)
		{
			warnings.AddRange(baseWarnings);
		}

		string[] errs = new string[warnings.Count];

		for (int i = 0; i < warnings.Count; i++)
		{
			errs.SetValue(warnings[i], i);
		}

		return errs;
	}
	
	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		
		QueryContext ??= GetChild<Context>(0);
		
		foreach (var child in QueryContext.GetChildren())
		{
			if (child is not Test t)
			{
				continue;
			}
				
			QueryTests.Add(t);
		}
		
	}
}
