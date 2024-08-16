using Godot;

using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectMina.EQS;

public partial class QueryPoint : RefCounted
{
	public Vector3 GlobalPosition;
	public float Score => TotalScore / TestCount;
	public float TotalScore = 0.0f;
	public int TestCount;

	public float AddScore(float score)
	{
		TotalScore += score;
		TestCount++;
		return Score;
	}
}

[Tool]
[GlobalClass]
public partial class EnvironmentQuery : EQSNode
{
	public Context QueryContext;
	public Godot.Collections.Array<Test> QueryTests = new();
	
	// this should return a variant that the RunEnvironmentQuery action can set as a result of it running.
	public async Task<Godot.Collections.Dictionary<Vector3, float>> RunQuery(AIControllerComponent querier)
	{
		if (Engine.IsEditorHint())
		{
			return new();
		}
		if (QueryContext == null || QueryTests == null || QueryTests.Count < 1)
		{
			if (EnableDebug)
			{
				GD.PushError("Can't run query. Query Context: ", QueryContext?.Name, ". Tests Count: ", QueryTests?.Count);
			}
			
			return new();
		}
		
		Array<QueryPoint> queryPoints = await QueryContext.GeneratePoints(querier);

		foreach (var test in QueryTests)
		{
			queryPoints = await test.RunTest(querier, queryPoints);
		}

		var sortedPoints = new Godot.Collections.Dictionary<Vector3, float>();
		
		if (queryPoints.Count < 1)
		{
			return sortedPoints;
		}
		GD.Randomize();
		queryPoints.Shuffle();
		queryPoints = new(queryPoints.OrderByDescending(point => point.TotalScore / point.TestCount));

		foreach (var point in queryPoints)
		{
			if (!sortedPoints.ContainsKey(point.GlobalPosition))
			{
				sortedPoints.Add(point.GlobalPosition, point.TotalScore / point.TestCount);	
			}
		}

		return sortedPoints;
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
