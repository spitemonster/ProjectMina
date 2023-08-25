using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class EquipmentManager : Node
{
	public Vector3 AimPosition;
	public Node3D EquippedItem { get; private set; }
	public Equipment.EquipmentType EquippedItemType { get; private set; }

	[Export]
	public Marker3D equipmentPosition;

	private CharacterBase _owner;
	private WeaponBase _equippedWeapon;
	private Node3D _equippedTool;

	public void EquipItem(Node3D item)
	{
		if (!item.HasNode("Equipment"))
		{
			GD.PushError("Attempted to equip item that was not equipment");
			return;
		}

		EquippedItem = item;
		EquippedItemType = EquippedItem.GetNode<Equipment>("Equipment").Type;

		switch (EquippedItemType)
		{
			case Equipment.EquipmentType.Weapon:
				WeaponBase _weapon = (WeaponBase)EquippedItem;
				WeaponBase.WeaponType weaponType = _weapon.weaponType;

				switch (weaponType)
				{
					case (WeaponBase.WeaponType.Melee):
						break;
					case (WeaponBase.WeaponType.Ranged):
						RangedWeapon _rangedWeapon = (RangedWeapon)EquippedItem;
						_rangedWeapon.GetNode<Equipment>("Equipment").Equip(_owner);
						break;
				}

				_equippedTool = null;
				break;
			case Equipment.EquipmentType.Tool:
				_equippedTool = EquippedItem;
				_equippedWeapon = null;
				break;
		}
	}

	public void DropEquippedItem()
	{
		_equippedWeapon = null;
	}

	public override void _Ready()
	{
		_owner = GetOwner<CharacterBase>();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (EquippedItem != null && equipmentPosition != null)
		{
			// Vector3 currentRotation = equipmentPosition.GlobalRotation;
			// currentRotation.X = 0;
			// currentRotation.Z = 0;
			// equipmentPosition.GlobalRotation = currentRotation;
			EquippedItem.GlobalTransform = equipmentPosition.GlobalTransform;
		}
	}
}
