using Godot;
using System;

namespace ProjectMina;

public partial class EnvironmentManager : ManagerBase
{
	[Export] public DirectionalLight3D Sun { get; protected set; }

	public override void _Ready()
	{
		base._Ready();
		if (Sun == null)
		{
			System.Diagnostics.Debug.Assert(false, "no sun");
		}
	}

	public override void _Process(double delta)
	{
		Vector3 sunRotation = Sun.GlobalRotation;
		sunRotation.X += .35f * (float)delta;

		Sun.GlobalRotation = sunRotation;
	}
}
