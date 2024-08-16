using Godot;
using System;
using ProjectMina;

[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public string WeaponName = "Weapon";
    [Export] public EWeaponType WeaponType { get; protected set; } = EWeaponType.Melee;
}
