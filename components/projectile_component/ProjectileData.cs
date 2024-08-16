using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class ProjectileData : Resource
{
	[Export] public PackedScene ImpactParticle;
	[Export] public float Damage = 1f;
	/// <summary>
	/// duration this projectile will stay alive before destroying
	/// </summary>
	[Export] public float Lifetime = 3f;
	/// <summary>
	/// velocity of the projectile once it is fired
	/// </summary>
	[Export] public float Speed = 200f;
	[Export] public Array<AudioStreamWav> ImpactSounds;
	[Export] public Array<AudioStreamWav> TravelSounds;
}
