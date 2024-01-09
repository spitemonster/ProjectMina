using Godot;

namespace ProjectMina;

public partial class EquipmentComponent : ComponentBase
{
	[Signal] public delegate void WeaponEquippedEventHandler(EquippableComponent weapon);
	[Signal] public delegate void ToolEquippedEventHandler(EquippableComponent tool);
	[Signal] public delegate void WeaponUnequippedEventHandler();
	[Signal] public delegate void ToolUnequippedEventHandler();
	[Export] public Node3D LeftHandSlot { get; protected set; }
	[Export] public Node3D RightHandSlot { get; protected set; }
	[Export] private bool _requestFirstPersonAnims = false;

	private EquippableComponent _leftHandItem;
	private EquippableComponent _rightHandItem;

	private CharacterBase _owner;

	public bool HasWeapon()
	{
		return _rightHandItem != null;
	}

	public void Equip(EquippableComponent equippable)
	{
		GD.Print("equipping");
		switch (equippable.Type)
		{
			case EquippableComponent.EquipmentType.Tool:
				EquipTool(equippable);
				break;
			case EquippableComponent.EquipmentType.Weapon:
				GD.Print("is weapon");
				EquipWeapon(equippable);
				break;
			default:
				break;
		}
	}

	public void EquipTool(EquippableComponent tool)
	{
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
		_leftHandItem?.Unequip();
		EmitSignal(SignalName.ToolUnequipped, _leftHandItem);
	}

	public void EquipWeapon(EquippableComponent weapon)
	{
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
