using Godot;

namespace ProjectMina;

/// <summary>
/// This component attaches to a PlayerCharacter and handles interaction events from the PlayerCharacter that owns it
/// handles passing events to the correct components or executing correct interaction functionality for equipping, grabbing, etc
/// </summary>

public partial class InteractionComponent : ComponentBase
{
	public enum InteractionType
	{
		None,
		Grab,
		Use,
		Equip
	};
	
	[Signal] public delegate void InteractedEventHandler(Node3D item);
	[Signal] public delegate void GrabbedEventHandler(RigidBody3D item);
	[Signal] public delegate void DroppedEventHandler(RigidBody3D item, bool thrown);

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
			_player.CharacterEquipment.EquipWeapon(weaponComponent);
			return true;
		}
		
		if (target.GetNodeOrNull<ToolComponent>("Tool") is { } toolComponent && _player.CharacterEquipment != null)
		{ 
			_player.CharacterEquipment.EquipTool(toolComponent);
			return true;
		}

		if (target.GetNodeOrNull<EquippableComponent>("Equippable") is { } equippableComponent && _player.CharacterEquipment != null)
		{ 
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

	}

	private void Use(Node3D targetItem)
	{

	}

	private void Equip()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}

	private void Take()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}
}
