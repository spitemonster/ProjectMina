using Godot;
using System;

namespace ProjectMina;

public partial class WeaponStats : Resource
{
    

    [Export] public float FireRate = 1f;
    [Export] public bool Automatic = false;
    [Export] public int MaxAmmo = 8;
    [Export] public int MaxReserve = 56;
    [Export] public double ReloadDuration = .5;
    [Export] public PackedScene BulletScene;
    
}
