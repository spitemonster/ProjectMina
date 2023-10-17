using Godot;
using System;

// [Tool]
[GlobalClass]
public partial class EnvironmentManager : Node
{
	[Export] public DirectionalLight3D Sun { get; protected set; }
	[Export] private bool _enabled = true;


	public override void _Ready()
	{
		if (Sun == null)
		{
			System.Diagnostics.Debug.Assert(false, "no sun");
		}

		SetProcess(_enabled);
	}

	public override void _Process(double delta)
	{
		Vector3 sunRotation = Sun.GlobalRotation;
		sunRotation.X += .35f * (float)delta;

		Sun.GlobalRotation = sunRotation;
	}
}
