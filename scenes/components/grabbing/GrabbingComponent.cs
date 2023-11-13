using Godot;
using ProjectMina;

[GlobalClass]
public partial class GrabbingComponent : ComponentBase
{
	public enum GrabWeight : uint {
		None,
		Light,
		Heavy
	}
	
	[Signal] public delegate void ItemGrabbedEventHandler(Node3D grabbedItem);
	[Signal] public delegate void ItemDroppedEventHandler(Node3D droppedItem, bool fromBreak = false);
	
	[Export] public Generic6DofJoint3D GrabJoint;
	[Export] public StaticBody3D GrabAnchor;
	
	[Export] public float GrabStrength = 10.0f;

	private static RigidBody3D _grabbedItem;

	private GrabWeight _grabState = GrabWeight.None;
	
	public bool CanGrab(Node3D item = null)
	{
		GD.Print("can grab: ", item, "?: ", item is RigidBody3D);
		
		if (item != null)
		{
			return item is RigidBody3D;
		}

		return false;
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
			GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
			GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
		}
		else
		{
			_grabState = GrabWeight.Light;
		}
		
		EmitSignal(SignalName.ItemGrabbed, _grabbedItem);
		return true;
	}

	public void ReleaseGrabbedItem()
	{
		EmitSignal(SignalName.ItemDropped, _grabbedItem);
		_grabbedItem = null;
		GrabJoint.NodeB = null;

		if (_grabState == GrabWeight.Heavy)
		{
			GrabJoint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			GrabJoint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
		}

		_grabState = GrabWeight.None;
	}
}
