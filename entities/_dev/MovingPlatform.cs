using Godot;
using System;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class MovingPlatform : Node3D
{
	[Export] public float TravelRate = 1.5f;
	private PathFollow3D _pathFollow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_pathFollow = GetNode<PathFollow3D>("%PathFollow3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_pathFollow.Progress += TravelRate * (float)delta;
	}
}
