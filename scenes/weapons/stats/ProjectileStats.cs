using Godot;

namespace ProjectMina;

public partial class ProjectileStats : Resource
{
	[Export] public Resource ImpactParticle;
	[Export] public double Damage = 1.0;
	[Export] public double Lifetime = 3.0;
	[Export] public double Speed = 500.0;
	[Export] public Godot.Collections.Array<AudioStreamWav> ImpactSounds;
	[Export] public Godot.Collections.Array<AudioStreamWav> TravelSounds;
}
