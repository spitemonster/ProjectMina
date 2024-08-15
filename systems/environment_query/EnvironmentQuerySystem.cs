using System.Collections.Generic;
using Godot;
using System.Linq;
using Godot.Collections;

namespace ProjectMina.EQS;

public partial class QueryNodeInfo : RefCounted
{
	public Vector3 Position;
}

[Tool]
[GlobalClass]
public partial class EnvironmentQuerySystem : Node3D
{
	[Signal] public delegate void QueryRequestedEventHandler(int requestId);
	[Signal] public delegate void QueryRequestFulfilledEventHandler(int requestId);
	[Signal] public delegate void QueryRequestRevokedEventHandler(int requestId);

	[Export] public bool Enabled = true;
	[Export] public float LevelBounds = 50f;
	[Export] public float PointSpacing = 10f;
	[Export] public float MaxAdjustmentDistance = 1f;
	[Export] public NavigationRegion3D NavRegion;
	
	private int _currentRequestIdIncrementor = 0;
	private System.Collections.Generic.Dictionary<int, QueryRequest> _queryRequests = new();
	private Octree _octree;

	private bool _octreeInitialized = false;

	public int RequestQuery(AIControllerComponent querier, EnvironmentQuery environmentQuery)
	{
		_currentRequestIdIncrementor++;
		QueryRequest request = new(_currentRequestIdIncrementor, querier, environmentQuery);
		_queryRequests.Add(_currentRequestIdIncrementor, request);
		return _currentRequestIdIncrementor;
	}

	public List<Vector3> GetPointsNearPosition(Vector3 position, float radius)
	{
		return _octree.QueryPosition(position, radius);
	}

	public bool RevokeQueryRequest(int ID)
	{
		if (_queryRequests[ID] == null) return false;
		
		_queryRequests.Remove(ID);
		return true;
	}

	public override void _Ready()
	{
		if (!Enabled)
		{
			return;
		}
		
		NavigationServer3D.MapChanged += _Init;
	}
	
	private void _Init(Rid map)
	{
		if (Global.Data == null || Global.Data.CurrentLevel == null)
		{
			return;
		}

		var currentMap = NavRegion.GetNavigationMap();

		if (map != currentMap)
		{
			return;
		}

		if (_octreeInitialized)
		{
			return;
		}
		
		_octreeInitialized = true;
		_octree = new Octree(GlobalPosition, LevelBounds, 1f);

		var points = _InitQueryPoints(currentMap);

		foreach (var point in points)
		{
			_octree.Add(point);
		}

		Global.Data.SetEnvironmentQuerySystem(this);
	}

	private List<Vector3> _InitQueryPoints(Rid currentMap)
	{
		var points = new List<Vector3>();

		for (var x = LevelBounds * -1; x < LevelBounds; x += PointSpacing)
		{
			// y starts at the node's y position just for clarity
			for (var y = 0f; y < LevelBounds; y += PointSpacing)
			{
				for (var z = LevelBounds * -1; z < LevelBounds; z += PointSpacing)
				{
					var p = new Vector3(x, y, z);
					var closest = NavigationServer3D.MapGetClosestPoint(currentMap, p);
					if (closest == p || p.DistanceTo(closest) <= MaxAdjustmentDistance)
					{
						// DebugDraw.Sphere(p, 1f, Colors.Red, 120f);
						points.Add(closest);	
					} 
					
				}
			}	
		}

		return points;
	}

	public override void _PhysicsProcess(double delta)
	{
	}
}
