using Godot;
using System;

namespace ProjectMina;

public partial class UsableComponent : InteractableComponent
{
    public override bool Interact(CharacterBase character)
    {
        return base.Interact(character);
    }
}
