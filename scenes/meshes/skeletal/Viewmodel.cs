using Godot;
using System;

public partial class Viewmodel : Node3D
{
	[Export] public SkeletonIK3D LeftArmIK { get; protected set; }
	[Export] public SkeletonIK3D RightArmIK { get; protected set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LeftArmIK?.Start();
		RightArmIK?.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
