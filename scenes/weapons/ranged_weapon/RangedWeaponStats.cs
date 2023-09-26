using Godot;
namespace ProjectMina;

[GlobalClass]
public partial class RangedWeaponStats : Resource
{
	/// <summary>
	/// The number of projectiles fired from this ranged weapon per second. A value of 1.0 would result in the weapon being able to fire 1 round per second.
	/// </summary>
	[Export] public double FireRate = 1.0;
	/// <summary>
	/// Whether or not the character using it must manually fire or if it will sustain fire.
	/// </summary>
	[Export] public bool Automatic = false;
	[Export] public int MaxAmmo = 10;
	[Export] public Resource Projectile;
	[Export] public double ReloadDuration = .5;
	[Export] public Godot.Collections.Array<AudioStreamWav> FireSounds;
	[Export] public Godot.Collections.Array<AudioStreamWav> ReloadSounds;
}
