using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class SpatialAudioSystem : Node
{
	[Signal] public delegate void SpatialAudioInitializedEventHandler();
	public static SpatialAudioSystem Instance;
	
	public SpatialAudioComponent Listener { get; private set; }
	
	private AStar3D _audioMap;
	private AStar3D _roomMap;
	private AStar3D _portalMap;

	private SpatialAudioNode[] _nodes;
	private SpatialAudioRoom[] _rooms;
	private SpatialAudioPortal[] _portals;

	public override void _EnterTree()
	{
		if (Instance != null)
		{
			QueueFree();
		}

		Instance = this;
	}
	
	public override void _Ready()
	{
		CallDeferred("_Init");
	}

	private void _Init()
	{
		Global.Data.PlayerSet += _InitAudioMap;
	}

	// sets up a* grids
	// audio map contains both rooms and portals and portals are used as edges between rooms
	// room map and portal map are used exclusively for quickly finding the nearest room or portal node
	private void _InitAudioMap(PlayerCharacter player)
	{
		var nodes = GetTree().GetNodesInGroup("spatial_audio_nodes");

		if (nodes.Count < 2)
		{
			return;
		}

		Listener = player.GetNode<SpatialAudioComponent>("%SpatialAudioComponent");

		if (Listener == null)
		{
			GD.PushError(" no listener ");
			return;
		}
		
		_audioMap = new();
		_roomMap = new();
		_portalMap = new();
		
		List<SpatialAudioNode> spatialNodes = new List<SpatialAudioNode>();
		
		foreach (var node in nodes)
		{
			if (node is SpatialAudioNode s)
			{
				var idx = (uint)_audioMap.GetAvailablePointId();
				s.SetIndex(idx);
				spatialNodes.Add(s);
				_audioMap.AddPoint(idx, s.GlobalPosition);
			}
		}

		_nodes = new SpatialAudioNode[spatialNodes.Count];
		_rooms = new SpatialAudioRoom[spatialNodes.Count];
		_portals = new SpatialAudioPortal[spatialNodes.Count];
		
		for (int i = 0; i < spatialNodes.Count; i++)
		{
			var spatialNode = spatialNodes[i];
			
			_nodes[spatialNode.Index] = spatialNode;
			
			if (spatialNode is SpatialAudioRoom room)
			{
				_rooms[i] = room;
				_roomMap.AddPoint(room.Index, room.GlobalPosition);

				foreach (var portal in room.Portals)
				{
					_audioMap.ConnectPoints(spatialNode.Index, portal.Index);
				}
			}
			else if (spatialNode is SpatialAudioPortal portal)
			{
				_portals[i] = portal;
				_portalMap.AddPoint(portal.Index, portal.GlobalPosition);
			}
		}
		
		GD.Print(_nodes);

		EmitSignal(SignalName.SpatialAudioInitialized);
	}

	public SpatialAudioNode[] GetNodePathToListener(Vector3 origin)
	{
		var originRoom = _roomMap.GetClosestPoint(origin);
		var listenerRoom = _roomMap.GetClosestPoint(Listener.GlobalPosition);
		var idPath = _audioMap.GetIdPath(originRoom, listenerRoom);

		var spatialNodes = new SpatialAudioNode[idPath.Length];

		for (int i = 0; i < idPath.Length; i++)
		{
			spatialNodes[i] = _nodes[idPath[i]];
		}

		return spatialNodes;
	}
	
	public SpatialAudioPortal GetNearestPortal(Vector3 position)
	{
		var idx = (int)_portalMap.GetClosestPoint(position);
		return _portals[idx];
	}

	public SpatialAudioRoom GetNearestRoom(Vector3 position)
	{
		var idx = _roomMap.GetClosestPoint(position);
		return _rooms[idx];
	}

	public SpatialAudioNode GetNearestSpatialNode(Vector3 position)
	{
		var idx = _audioMap.GetClosestPoint(position);
		return _nodes[idx];
	}
}
