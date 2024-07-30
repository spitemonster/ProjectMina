using Godot;

namespace ProjectMina;

public partial class GrabbingComponent : ComponentBase
{
	public enum EGrabWeight : uint
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

	public RigidBody3D GrabbedItem { get; protected set; }

	private EGrabWeight _grabState = EGrabWeight.None;

	private DevMonitor _devMonitor;
	private DevMonitor _weightMonitor;
	public override void _Ready()
	{
		CallDeferred("_Init");
	}

	private void _Init()
	{
		_devMonitor = Dev.UI.AddDevMonitor("Grabbing: ", Colors.Aqua, "Player:Grabbing");
		_weightMonitor = Dev.UI.AddDevMonitor("Grab Weight: ", Colors.Aqua, "Player:Grabbing");
	}

	public static bool CanGrab(Node3D item = null)
	{
		return item is RigidBody3D;
	}

	public bool IsGrabbing()
	{
		return GrabbedItem != null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsGrabbing() || GrabAnchor == null)
		{
			_devMonitor.SetValue("none");
			_weightMonitor.SetValue("none");
			return;
		}

		float grabbedItemDistance = (GrabbedItem.GlobalPosition - GrabAnchor.GlobalPosition).Length();

		if (grabbedItemDistance > 0.6f)
		{
			ReleaseGrabbedItem();
			return;
		}

		var grabbedItemCurrentPosition = GrabbedItem.GlobalPosition;
		var targetGrabbedItemPosition = GrabAnchor.GlobalPosition;
		var targetGrabbedItemLinearVelocity =
			(targetGrabbedItemPosition - grabbedItemCurrentPosition) * GrabStrength;

		// should check object weight here and not apply 
		if (_grabState == EGrabWeight.Heavy)
		{
			targetGrabbedItemLinearVelocity.Y = GrabbedItem.LinearVelocity.Y;
		}

		_devMonitor.SetValue(GrabbedItem.Name);
		_weightMonitor.SetValue(GrabbedItem.Mass.ToString());
		GrabbedItem.LinearVelocity = targetGrabbedItemLinearVelocity;
	}

	public bool GrabItem(RigidBody3D item)
	{
		if (IsGrabbing())
		{
			GD.PushError("Attempted to grab item while already grabbing item");
			return false;
		}

		GrabJoint.NodeB = item.GetPath();
		GrabbedItem = item;

		if (GrabbedItem.Mass > 100)
		{
			_grabState = EGrabWeight.Heavy;

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
			_grabState = EGrabWeight.Light;
		}

		EmitSignal(SignalName.ItemGrabbed, GrabbedItem);
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
			return Vector3.Axis.X;
		}

		if (maxDot == dotY)
		{
			return Vector3.Axis.Y;
		}

		return Vector3.Axis.Z;
	}

	public void ReleaseGrabbedItem(Vector3 force = default, float strengthRatio = 1.0f)
	{
		
		GrabJoint.NodeB = null;

		if (force.Length() > 0 && _grabState != EGrabWeight.Heavy)
		{
			GrabbedItem.ApplyImpulse(force * strengthRatio);
		}
		
		if (_grabState == EGrabWeight.Heavy)
		{
			GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			GrabJoint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
		}

		_grabState = EGrabWeight.None;
		
		EmitSignal(SignalName.ItemDropped, GrabbedItem, false);
		GrabbedItem = null;
		
	}
}
