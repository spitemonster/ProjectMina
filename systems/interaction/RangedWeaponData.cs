using Godot;
using Godot.Collections;
using ProjectMina;

[GlobalClass]
public partial class RangedWeaponData : WeaponData
{
    [ExportCategory("Ammo")]
    [Export] public int AmmoPerClip = 8;
    [Export] public int MaxReserveClips = 6;
    
    [ExportCategory("Firing")]
    [Export] public float FireRate = 1f;
    [Export] public bool Automatic = false;
    [Export] public float ReloadDuration = .5f;

    [Export] public float Spread = 3f;
    
    [Export] public bool Projectile = true;
    [Export] public PackedScene ProjectileScene;

    [Export] public AudioStream[] FiringSounds;
    [Export] public AudioStream[] EmptySounds;
    
    public int MaxReserveAmmo => MaxReserveClips * AmmoPerClip;

    public RangedWeaponData()
    {
        WeaponType = EWeaponType.Ranged;
    }
}
