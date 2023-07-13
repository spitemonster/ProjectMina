using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class Equipment : Node
{
	[Signal]
	public delegate void EquippedEventHandler(CharacterBase equippingCharacter);

	public void Equip(CharacterBase equippingCharacter)
	{
		EmitSignal(SignalName.Equipped, equippingCharacter);
	}
}
