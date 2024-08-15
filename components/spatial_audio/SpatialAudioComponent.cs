using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class SpatialAudioComponent : ComponentBase3D
{
	[Export] protected float UpdateFrequency = .25f;
	// [Export] protected Area3D 
	private CharacterBase _character;
	private Timer _updateTimer;
	public override void _Ready()
	{

	}

	private void _UpdateSpatialAudio()
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Active)
		{
			return;
		}
		
		var node = SpatialAudioSystem.Instance.GetNearestSpatialNode(GlobalPosition);
		
		DebugDraw.Sphere(GlobalPosition, .25f, Colors.Aqua);
		DebugDraw.Sphere(node.GlobalPosition, 1f, Colors.Coral);
	}
}
