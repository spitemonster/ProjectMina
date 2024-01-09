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
	
	public Node3D FocusedItem;
	[Signal] public delegate void FocusedEventHandler(Node3D item);
	[Signal] public delegate void LostFocusEventHandler(Node3D item);
	[Signal] public delegate void InteractedEventHandler(Node3D item);
	[Signal] public delegate void GrabbedEventHandler(RigidBody3D item);
	[Signal] public delegate void DroppedEventHandler(RigidBody3D item, bool thrown);
	[Signal] public delegate void InteractionStateChangedEventHandler(InteractionType newState);

	[Export] protected Area3D _interactionCollision;

	private PlayerCharacter _player;
	
	public override void _Ready()
	{
		base._Ready();
		
		if (!Active || Engine.IsEditorHint())
		{
			return;
		}
		
		_player = GetOwner<PlayerCharacter>();
		
		// if (playerCharacter != null && playerCharacter.CharacterAttention != null)
		// {
		// 	playerCharacter.CharacterAttention.FocusChanged += (newFocus, previousFocus) =>
		// 	{
		// 		CheckInteraction(newFocus);
		// 	};
		// }
	}

	public bool CanInteract()
	{
		return _player.CharacterAttention.CurrentFocus is Node3D i &&
		       (i.HasNode("InteractableComponent") || i.HasNode("Equippable") || i.HasNode("Weapon") || i.HasNode("Tool"));
	}

	public bool Interact(bool isAlt = false)
	{
		if (!CanInteract())
		{
			return false;
		}

		Node3D target = (Node3D)_player.CharacterAttention.CurrentFocus;

		if (target == null)
		{
			return false;
		}
		
		if (target.GetNodeOrNull<WeaponComponent>("Weapon") is { } weaponComponent && _player.CharacterEquipment != null)
		{ 
			GD.Print("should be equipping a weapon");
			_player.CharacterEquipment.Equip(weaponComponent);
			return true;
		}
		
		if (target.GetNodeOrNull<ToolComponent>("Tool") is { } toolComponent && _player.CharacterEquipment != null)
		{ 
			GD.Print("tool");
			_player.CharacterEquipment.Equip(toolComponent);
			return true;
		}

		if (target.GetNodeOrNull<EquippableComponent>("Equippable") is { } equippableComponent && _player.CharacterEquipment != null)
		{ 
			GD.Print("FUCK");
			_player.CharacterEquipment.Equip(equippableComponent);
            return true;
		}
		
		if (target.GetNode<InteractableComponent>("InteractableComponent") is { } interactableComponent)
		{
			interactableComponent.Interact(_player);
            return true;
		}
		
		return false;
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
