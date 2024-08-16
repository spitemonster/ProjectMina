using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class MeleeWeaponData : WeaponData
{
    public MeleeWeaponData()
    {
        WeaponType = EWeaponType.Melee;
    }
}
