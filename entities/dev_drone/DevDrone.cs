using Godot;
using Godot.Collections;

namespace ProjectMina;

/// <summary>
/// general free look/fly camera for probing and exploring without needing to walk or collide
/// </summary>
[GlobalClass]
public partial class DevDrone : CharacterBody3D
{
	[Export] public float FlySpeed = 3.0f;
	
	private float _totalPitch = 0.0f;
	public override void _Ready()
	{
		PlayerInput.Manager.MouseMove += _HandleLook;
		PlayerInput.Manager.ActionPressed += _OnActionPressed;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var controlInput = PlayerInput.GetInputDirection();
		var direction = (GlobalTransform.Basis * new Vector3(controlInput.X, 0, controlInput.Y)).Normalized();
		float currentSpeed = FlySpeed;

		if (Input.IsActionPressed("lean_left"))
		{
			direction.Y -= 1.5f;
		}

		if (Input.IsActionPressed("lean_right"))
		{
			direction.Y += 1.5f;
		}
		
		if (Input.IsActionPressed("run"))
		{
			currentSpeed *= 1.5f;
		}

		Velocity = direction * currentSpeed;

		_ = MoveAndSlide();

		// var nearbyPoints = Global.EQS.GetPointsNearPosition(GlobalPosition, 5f);
		//
		// foreach (var point in nearbyPoints)
		// {
		// 	GD.Print(point);
		// 	var x = point;
		//
		// 	if (x != Vector3.Zero)
		// 	{
		// 		DebugDraw.Sphere(x, 1.125f, Colors.Red);
		// 	}
		// }
	}

	private void _OnActionPressed(StringName action)
	{
	}

	private void _HandleLook(Vector2 mouseDelta)
	{
		if (GetTree().Paused) return;
		
		var yaw = (float)(-mouseDelta.X * .125);
		var pitch = (float)(-mouseDelta.Y * .125);

		pitch = Mathf.Clamp(pitch, -90, 90);
		
		RotateY(Mathf.DegToRad(yaw));
		RotateObjectLocal(new Vector3(1,0,0), Mathf.DegToRad(pitch));
		
		// Rotation *= new Vector3(1.0f, 1.0f, 0.0f);
	}
}
