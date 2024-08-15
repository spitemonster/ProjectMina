using Godot;
using System;

namespace ProjectMina;
[GlobalClass]
public partial class SpatialAudioPlayer : AudioStreamPlayer3D
{
	[Export] public float UpdateFrequency = .05f;
	// the distance at which the player will begin transitioning from dry to wet
	[Export] public float MinDistance = 1f;
	// the distance at which the player will achieve full...wetness... :(
	[Export] public float MaxDistance = 10f;
	
	public SpatialAudioRoom CurrentRoom { get; protected set; }

	private float _distanceToListener;
	private bool _lineOfSightToListener;

	private float _currentUpdateFrequency;
	
	private SpatialAudioRoomSettings _busSettings;
	private int _busIndex;
	private StringName _busName;

	private Timer _updateTimer;

	private AudioEffectReverb _reverb;
	
	public override void _Ready()
	{
		SpatialAudioSystem.Instance.SpatialAudioInitialized += _Init;
		
		
	}

	public void _Init()
	{
		SpatialAudioRoom nearestRoom = SpatialAudioSystem.Instance.GetNearestRoom(GlobalPosition);

		if (nearestRoom == null)
		{
			GD.PushError("no nearest room");
			return;
		}
		
		DebugDraw.Box(nearestRoom.GlobalPosition, new Vector3(.5f, .5f, .5f), Colors.Red, 60f);

		_busIndex = AudioServer.GetBusCount();
		_busName = Name + _busIndex;
		_busSettings = nearestRoom.RoomSettings;
		AudioServer.AddBus(_busIndex);

		foreach (var effect in _busSettings.Effects)
		{
			AudioServer.AddBusEffect(_busIndex, effect);
		}
		
		AudioServer.SetBusName(_busIndex, _busName);
		
		SetBus(_busName);

		_reverb = (AudioEffectReverb)AudioServer.GetBusEffect(_busIndex, 0);

		_updateTimer = new()
		{
			WaitTime = UpdateFrequency,
			Autostart = true,
			OneShot = false
		};
		
		GetTree().Root.AddChild(_updateTimer);

		_updateTimer.Timeout += _UpdateSpatialAudio;
	}

	private void _UpdateSpatialAudio()
	{
		var nodes = SpatialAudioSystem.Instance.GetNodePathToListener(GlobalPosition);
		
		_distanceToListener = GlobalPosition.DistanceTo(SpatialAudioSystem.Instance.Listener.GlobalPosition);

		var wetness = Mathf.Clamp((_distanceToListener - (MinDistance + .5f)) / MaxDistance, 0 ,1);
		
		_reverb.Dry = 1 - wetness;
		
		GD.Print(wetness);

		HitResult los = Cast.Ray(GetWorld3D().DirectSpaceState, GlobalPosition, SpatialAudioSystem.Instance.Listener.GlobalPosition,
			new(), true, true);

		_lineOfSightToListener = los?.Collider == SpatialAudioSystem.Instance.Listener.Owner;
		
		// GD.Print("line of sight to listener?: ", _lineOfSightToListener, " collider: ", los?.Collider.Name);

		foreach (var node in nodes)
		{
			Node3D n = (Node3D)node;
			if (n == null)
			{
				continue;
			}
			
			switch (node)
			{
				case SpatialAudioRoom room:
					DebugDraw.Sphere(n.GlobalPosition, .75f, Colors.Red, .05f);
					break;
				case SpatialAudioPortal portal:
					DebugDraw.Sphere(n.GlobalPosition, .45f, Colors.Yellow, .05f);
					break;
			}
		}
	}
}
