using Godot;

namespace ProjectMina;

public partial class InteractionComponent : ComponentBase
{
	public enum InteractionType
	{
		None,
		Grab,
		Use,
		Equip
	};

	public bool CanEquip { get; private set; }
	// public bool CanInteract { get; private set; }
	// public Node3D FocusedItem => _focusedItem;

	[Signal] public delegate void FocusedEventHandler(Node3D item);
	[Signal] public delegate void LostFocusEventHandler(Node3D item);
	[Signal] public delegate void InteractedEventHandler(Node3D item);
	[Signal] public delegate void GrabbedEventHandler(RigidBody3D item);
	[Signal] public delegate void DroppedEventHandler(RigidBody3D item, bool thrown);
	[Signal] public delegate void InteractionStateChangedEventHandler(InteractionType newState);

	[Export] protected Area3D _interactionCollision;

	public bool CanInteract(Node3D target)
	{
		return target.HasNode("InteractableComponent");
	}

	public override void _Ready()
	{
		// base._Ready();
		//
		// if (!_active || Engine.IsEditorHint())
		// {
		// 	return;
		// }
		//
		// var playerCharacter = GetOwner<PlayerCharacter>();
		//
		// if (playerCharacter != null && playerCharacter.CharacterAttention != null)
		// {
		// 	playerCharacter.CharacterAttention.FocusChanged += (newFocus, previousFocus) =>
		// 	{
		//
		// 		if (newFocus == null)
		// 		{
		// 			if (_focusedItem == null)
		// 			{
		// 				return;
		// 			}
		// 			
		// 			LoseFocus();
		// 		}
		// 		else
		// 		{
		// 			CheckInteraction(newFocus);
		// 		}
		// 	};
		// }
	}

	public void Interact(Node3D target, bool isAlt = false)
	{
		GD.Print("interacting with item: ", target);
		// switch (_interactionState)
		// {
		// 	case InteractionType.Use:
		// 		Use(_focusedItem);
		// 		break;
		// 	case InteractionType.Equip:
		// 		equipmentManager.EquipItem(_focusedItem);
		// 		break;
		// 	case InteractionType.None:
		// 		break;
		// 	default:
		// 		break;
		// }
	}

	private void CheckInteraction(Node3D targetItem)
	{
		// if (_focusedItem != null)
		// {
		// 	return;
		// }
		//
		// var t = CheckCanFocus(targetItem);
		//
		// if (t != _interactionState && t != InteractionType.None)
		// {
		// 	_focusedItem = targetItem;
		// 	_interactionState = t;
		// 	EmitSignal(SignalName.InteractionStateChanged, (int)t);
		// 	EmitSignal(SignalName.Focused, targetItem);
		// }
		// else if (t != _interactionState && t == InteractionType.None)
		// {
		// 	_focusedItem = null;
		// 	_interactionState = t;
		// 	EmitSignal(SignalName.InteractionStateChanged, (int)t);
		// 	EmitSignal(SignalName.LostFocus, targetItem);
		// }
	}

	// private InteractionType CheckCanFocus(Node3D targetItem)
	// {
	// 	if (targetItem.HasNode("Equipment"))
	// 	{
	// 		CanInteract = true;
	// 		return InteractionType.Equip;
	// 	}
	// 	else if (targetItem.HasNode("InteractableComponent"))
	// 	{
	// 		return InteractionType.None;
	// 	}
	// 	else if (targetItem is RigidBody3D)
	// 	{
	// 		CanInteract = true;
	// 		return InteractionType.Grab;
	// 	}
	// 	else
	// 	{
	// 		return InteractionType.None;
	// 	}
	// }

	// private void CheckLoseFocus(Node3D targetItem)
	// {
	// 	// if (_focusedItem == targetItem)
	// 	// {
	// 	// 	LoseFocus();
	// 	// }
	// }
	//
	// private void LoseFocus()
	// {
	// 	// EmitSignal(SignalName.LostFocus, _focusedItem);
	// 	// _focusedItem = null;
	// 	// _interactionState = InteractionType.None;
	// 	// EmitSignal(SignalName.InteractionStateChanged, (int)_interactionState);
	// }

	private void Use(Node3D targetItem)
	{

	}

	// private bool Grab(RigidBody3D item)
	// {
	// 	if (_grabbedItem == null)
	// 	{
	// 		_grabbedItem = item;
	// 		_grabJoint.NodeB = _grabbedItem.GetPath();
	// 		return true;
	// 	}
	// 	
	// 	_grabJoint.NodeB = null;
	// 	_grabbedItem = null;
	//
	// 	return false;
	// }

	// private void Drop(float force = 1.0f)
	// {
	// 	if (_grabbedItem == null)
	// 	{
	// 		return;
	// 	}
	// 	
	// 	_grabbedItem = null;
	// 	_grabJoint.NodeB = null;
	// 	_focusedItem = null;
	// }

	private void Equip()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}

	private void Take()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}
}
