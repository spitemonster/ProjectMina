using Godot;

namespace ProjectMina;


[GlobalClass]
public partial class Interaction : Node
{
	[Signal] public delegate void UsedEventHandler(CharacterBase interactingCharacter);
	[Signal] public delegate void EndedUseEventHandler(CharacterBase interactingCharacter);

	public void Use(CharacterBase interactingCharacter)
	{
		EmitSignal(SignalName.Used, interactingCharacter);
	}

	public void EndUse(CharacterBase interactingCharacter)
	{
		EmitSignal(SignalName.EndedUse, interactingCharacter);
	}
}
