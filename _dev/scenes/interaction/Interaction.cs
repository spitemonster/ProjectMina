using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class Interaction : Node
{

	[Signal]
	public delegate void UsedEventHandler(CharacterBase interactingCharacter);

	public void Use(CharacterBase interactingCharacter)
	{
		EmitSignal(SignalName.Used, interactingCharacter);
	}
}
