using Godot;
using System;
using Godot.Collections;
using ProjectMina;

namespace ProjectMina;
[GlobalClass]
public partial class SpatialAudioPortal : SpatialAudioNode
{
	public float Openness = 0f;
	
	private Array<SpatialAudioRoom> _connectedRooms;

	public void UpdateOpenness(float openness)
	{
		Openness = openness;
	}

	public bool AddConnectedRoom(SpatialAudioRoom room)
	{
		if (_connectedRooms == null)
		{
			_connectedRooms = new();
		}

		if (_connectedRooms.Contains(room))
		{
			return false;
		}
		
		_connectedRooms.Add(room);
		return true;
	}

	public override void _Ready()
	{
		_connectedRooms = new();
	}
}
