using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class FootstepComponent : Node
{
	[Export] public AudioStreamPlayer3D Player { get; protected set; }
	[Export] public Array<AudioStream> FootstepSounds;
	
	private CharacterBase _character;
	
	public override void _Ready()
	{
		_character = GetOwner<CharacterBase>();

		if (_character == null || Player == null)
		{
			return;
		}

		_character.CharacterStepped += PlayFootstep;
	}

	public void PlayFootstep(PhysicsMaterial material)
	{
		var sound = FootstepSounds.PickRandom();
		
		Player?.SetStream(sound);
		Player?.Play();
	}
}
