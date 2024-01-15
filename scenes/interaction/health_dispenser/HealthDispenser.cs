using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class HealthDispenser : StaticBody3D
{
	private InteractableComponent _interactable;

	public override void _Ready()
	{
		_interactable = GetNode<InteractableComponent>("Interactable");
		_interactable.InteractionStarted += _HealCharacter;
	}

	private static void _HealCharacter(CharacterBase character)
	{
		GD.Print(character.Name, " used health dispenser");
		character.CharacterHealth?.FullyHeal();
	}
}
