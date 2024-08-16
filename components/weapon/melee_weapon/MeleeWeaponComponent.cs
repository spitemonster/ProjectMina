using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class MeleeWeaponComponent : WeaponComponent
{
    [Export] public HitboxComponent Hitbox { get; protected set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void Equip(CharacterBase equippingCharacter, Node3D slot)
    {
        base.Equip(equippingCharacter, slot);
        
        Hitbox.SetExclude(new() { Wielder.GetRid() });
    }

    protected override void Attack()
    {
        Hitbox?.EnableHit();
    }

    protected override void EndAttack()
    {
        Hitbox.DisableHit();
    }
}
