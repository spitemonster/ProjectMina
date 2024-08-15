using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class SpatialAudioRoom : SpatialAudioNode
{
	[Export] public SpatialAudioPortal[] Portals { get; private set; }
	[Export] public Area3D RoomBounds;
	[Export] public SpatialAudioRoomSettings RoomSettings { get; protected set; }

	private StringName _busName;

	public override void _Ready()
	{
	}
}
