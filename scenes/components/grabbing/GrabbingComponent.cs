using Godot;

namespace ProjectMina;

public partial class GrabbingComponent : ComponentBase
{
	public enum GrabWeight : uint
	{
		None,
		Light,
		Heavy
	}

	[Signal] public delegate void ItemGrabbedEventHandler(Node3D grabbedItem);
	[Signal] public delegate void ItemDroppedEventHandler(Node3D droppedItem, bool fromBreak = false);

	[Export] public Generic6DofJoint3D GrabJoint;
	[Export] public StaticBody3D GrabAnchor;

	[Export] public float GrabStrength = 10.0f;
	[Export] protected float MaximumThrowStrength = 500.0f;

	private RigidBody3D _grabbedItem;

	private GrabWeight _grabState = GrabWeight.None;

	public static bool CanGrab(Node3D item = null)
	{
		return item is RigidBody3D;
	}

	public bool IsGrabbing()
	{
		return _grabbedItem != null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsGrabbing())
		{
			return;
		}

		float grabbedItemDistance = (_grabbedItem.GlobalPosition - GrabAnchor.GlobalPosition).Length();

		if (grabbedItemDistance > 0.6f)
		{
			ReleaseGrabbedItem();
			return;
		}

		var grabbedItemCurrentPosition = _grabbedItem.GlobalPosition;
		var targetGrabbedItemPosition = GrabAnchor.GlobalPosition;
		var targetGrabbedItemLinearVelocity =
			(targetGrabbedItemPosition - grabbedItemCurrentPosition) * GrabStrength;

		// should check object weight here and not apply 
		if (_grabState == GrabWeight.Heavy)
		{
			targetGrabbedItemLinearVelocity.Y = _grabbedItem.LinearVelocity.Y;
		}

		_grabbedItem.LinearVelocity = targetGrabbedItemLinearVelocity;
	}

	public bool GrabItem(RigidBody3D item)
	{
		if (IsGrabbing())
		{
			GD.PushError("Attempted to grab item while already grabbing item");
			return false;
		}

		GrabJoint.NodeB = item.GetPath();
		_grabbedItem = item;

		if (_grabbedItem.Mass > 100)
		{
			_grabState = GrabWeight.Heavy;

			Vector3.Axis itemUp = GetClosesAxisToUp(item.GlobalTransform);

			if (itemUp == Vector3.Axis.X)
			{
				GrabJoint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
				GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
			}
			else if (itemUp == Vector3.Axis.Y)
			{
				GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
				GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
			}
			else if (itemUp != Vector3.Axis.Z)
			{
				GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
				GrabJoint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
			}
		}
		else
		{
			_grabState = GrabWeight.Light;
		}

		EmitSignal(SignalName.ItemGrabbed, _grabbedItem);
		return true;
	}

	/// <summary>
	/// Given a global space transform, it returns the axis closest to world up (Vector3(0,1,0))
	/// </summary>
	/// <param name="transform">input transform</param>
	/// <returns>Vector axis by name which is nearest to world up</returns>
	private static Vector3.Axis GetClosesAxisToUp(Transform3D transform)
	{
		var baseX = transform.Basis.X;
		var baseY = transform.Basis.Y;
		var baseZ = transform.Basis.Z;

		var dotX = Mathf.Abs(baseX.Dot(Vector3.Up));
		var dotY = Mathf.Abs(baseY.Dot(Vector3.Up));
		var dotZ = Mathf.Abs(baseZ.Dot(Vector3.Up));

		var maxDot = Mathf.Max(dotX, dotY);
		maxDot = Mathf.Max(maxDot, dotZ);

		if (maxDot == dotX)
		{
			GD.Print("closest up is X");
			return Vector3.Axis.X;
		}

		if (maxDot == dotY)
		{
			GD.Print("closest up is Y");
			return Vector3.Axis.Y;
		}

		GD.Print("closest up is Z");
		return Vector3.Axis.Z;
	}

	public void ReleaseGrabbedItem(Vector3 force = default, float strengthRatio = 1.0f)
	{
		
		GrabJoint.NodeB = null;

		if (force.Length() > 0 && _grabState != GrabWeight.Heavy)
		{
			_grabbedItem.ApplyImpulse(force * strengthRatio);
		}
		
		if (_grabState == GrabWeight.Heavy)
		{
			GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			GrabJoint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
		}

		_grabState = GrabWeight.None;
		
		EmitSignal(SignalName.ItemDropped, _grabbedItem);
		_grabbedItem = null;
		
	}
}
