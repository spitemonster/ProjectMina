using Godot;

namespace ProjectMina;

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
		GD.Print("equipped from equipment thing");
		EmitSignal(SignalName.Equipped, equippingCharacter);
	}
}
