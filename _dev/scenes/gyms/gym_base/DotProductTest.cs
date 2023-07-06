using System;
using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class DotProductTest : Node3D
{
	private Vector3 _forwardVector;
	private LabelValueRow _dot;

	public override void _Ready()
	{
		base._Ready();
		_forwardVector = GlobalTransform.Basis.Z;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector3 toTarget = (Global.Data.Player.GlobalPosition - GlobalPosition).Normalized();
		float dotProduct = _forwardVector.Dot(toTarget);
		float angle = Mathf.Acos(dotProduct) * (180.0f / Mathf.Pi);
		float roundedAngle = Mathf.Clamp(Mathf.Round(angle), 0, 90);

		float clampedRange = 1 - Mathf.Round((roundedAngle / 90) * 100) / 100;
		_dot?.SetValue(clampedRange.ToString());
	}
}
