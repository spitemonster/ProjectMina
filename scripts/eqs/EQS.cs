using Godot;
using System.Linq;
using Godot.Collections;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class EQS : Node
{
	[Signal] public delegate void QueryRequestedEventHandler(int requestId);
	[Signal] public delegate void QueryRequestFulfilledEventHandler(int requestId);
	[Signal] public delegate void QueryRequestRevokedEventHandler(int requestId);
	public static EQS Generator { get; private set; }
	
	private int _currentRequestIdIncrementor = 0;
	private System.Collections.Generic.Dictionary<int, QueryRequest> _queryRequests = new();

	public int RequestQuery(AgentComponent querier, Query query)
	{
		_currentRequestIdIncrementor++;
		QueryRequest request = new(_currentRequestIdIncrementor, querier, query);
		_queryRequests.Add(_currentRequestIdIncrementor, request);
		return _currentRequestIdIncrementor;
	}

	public bool RevokeQueryRequest(int ID)
	{
		if (_queryRequests[ID] != null)
		{
			_queryRequests[ID].Free();
			_queryRequests.Remove(ID);
			return true;
		}

		return false;
	}
	
	public override void _EnterTree()
	{
		if (Generator != null)
		{
			QueueFree();
		}
		Generator = this;
	}

	public override void _PhysicsProcess(double delta)
	{
		// base._PhysicsProcess(delta);
		//
		// if (Engine.IsEditorHint())
		// {
		// 	return;
		// }
		//
		// if (_queryRequests.Count < 1)
		// {
		// 	return;
		// }
		//
		// // some stupid voodoo to get the first element in the dictionary.
		// var key = _queryRequests.Keys.ToArray()[0];
		// var currentRequest = _queryRequests[key];
		// _queryRequests.Remove(key);
		//
		// await currentRequest.Query.RunQuery(currentRequest.Querier);
	}
}
