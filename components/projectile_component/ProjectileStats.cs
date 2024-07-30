using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class ProjectileStats : Resource
{
	[Export] public Resource ImpactParticle;
	[Export] public float Damage = 1f;
	[Export] public float Lifetime = 3f;
	[Export] public float Speed = 200f;
	[Export] public Godot.Collections.Array<AudioStreamWav> ImpactSounds;
	[Export] public Godot.Collections.Array<AudioStreamWav> TravelSounds;
}
