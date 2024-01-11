using Godot;

namespace ProjectMina;

/// <summary>
///  this component attaches to a characterbase and manages its equipment, that is the items it has in its left and right hand slots
///  as opposed to its inventory which is items in its possession but ont actively equipped 
/// </summary>

public partial class EquipmentComponent : ComponentBase
{
	[Signal] public delegate void WeaponEquippedEventHandler(WeaponComponent weapon);
	[Signal] public delegate void ToolEquippedEventHandler(EquippableComponent tool);
	[Signal] public delegate void WeaponUnequippedEventHandler();
	[Signal] public delegate void ToolUnequippedEventHandler();
	[Export] public Node3D LeftHandSlot { get; protected set; }
	[Export] public Node3D RightHandSlot { get; protected set; }
	
	/// <summary>
	/// if this equipment component is attached to a first person character, such as the player character
	/// we should request first person-focused animations
	/// for most characters this is not the case
	/// </summary>
	[Export] private bool _requestFirstPersonAnims = false;

	private EquippableComponent _leftHandItem;
	private EquippableComponent _rightHandItem;

	private CharacterBase _owner;

	public bool HasWeapon()
	{
		return _rightHandItem != null;
	}

	public bool HasTool()
	{
		return _leftHandItem != null;
	}

	public void Equip(EquippableComponent equippable)
	{
		if (EnableDebug)
		{
			GD.Print("Equipping via generic equip function: ");
		}
		switch (equippable.Type)
		{
			case EquippableComponent.EquipmentType.Tool:
				EquipTool(equippable);
				break;
			case EquippableComponent.EquipmentType.Weapon:
				EquipWeapon(equippable);
				break;
			default:
				break;
		}
	}
	
	public void EquipTool(EquippableComponent tool)
	{
		if (EnableDebug)
		{
			GD.Print("Equipping tool: ", tool.Name);	
		}
		
		if (_leftHandItem != null)
		{
			DropTool();
		}

		_leftHandItem = tool;
		_leftHandItem.Equip(_owner, LeftHandSlot);
		EmitSignal(SignalName.ToolEquipped, _leftHandItem);
	}

	public void DropTool()
	{
		if (EnableDebug)
		{
			GD.Print("dropping previously equipped tool");
		}

		_leftHandItem?.Unequip();
		EmitSignal(SignalName.ToolUnequipped, _leftHandItem);
	}

	public void EquipWeapon(EquippableComponent weapon)
	{
		if (EnableDebug)
		{
			GD.Print("Equipping weapon: ", weapon.Name);	
		}
		
		if (_rightHandItem != null)
		{
			DropWeapon();
		}

		_rightHandItem = weapon;
		_rightHandItem.Equip(_owner, RightHandSlot);
		EmitSignal(SignalName.WeaponEquipped, _rightHandItem);
	}

	public void UseWeapon()
	{
		if (_rightHandItem == null)
		{
			return;
		}

		if (_rightHandItem.CanUse)
		{
			GD.Print("should attack");
			GD.Print();
		}
	}

	public void DropWeapon()
	{
		_rightHandItem?.Unequip();
		EmitSignal(SignalName.WeaponUnequipped, _rightHandItem);
	}
	
	public override void _Ready()
	{
		_owner = GetOwner<CharacterBase>();
	}

	public override void _Process(double delta)
	{
	}
}
