using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class Equipment : Node
{
	public enum EquipmentType
	{
		Weapon,
		Tool
	};

	[Export]
	public EquipmentType Type = EquipmentType.Tool;

	[Signal]
	public delegate void EquippedEventHandler(CharacterBase equippingCharacter);

	public void Equip(CharacterBase equippingCharacter)
	{
		EmitSignal(SignalName.Equipped, equippingCharacter);
	}
}
