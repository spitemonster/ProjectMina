using Godot;
using System;

namespace ProjectMina;

public partial class UsableComponent : InteractableComponent
{
    public override bool Interact(CharacterBase character)
    {
        GD.Print("interacting with usable component");
        return base.Interact(character);
    }
}
