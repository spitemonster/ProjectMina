using Godot;
namespace ProjectMina;

[GlobalClass]
public partial class RangedWeaponStats : Resource
{
	[Export] public Resource Mesh;
	[Export] public PackedScene MuzzleParticle;

	/// <summary>
	/// Whether or not this weapon should use raycasts rather than projectiles
	/// </summary>
	[Export] public bool Projectile = true;
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Whether or not the character using it must manually fire or if it will sustain fire. Note; using this places refire time entirely in the hands of the animation player.
	/// </summary>
	[Export] public bool Automatic = false;
	[Export] public int MaxAmmo = 8;
	[Export] public int MaxReserve = 36;
	
	[Export] public double ReloadDuration = .5;
	
	[Export] public Godot.Collections.Array<AudioStreamWav> FireSounds;
	[Export] public Godot.Collections.Array<AudioStreamWav> ReloadSounds;

	[Export] public float Spread = 1.0f;
}
