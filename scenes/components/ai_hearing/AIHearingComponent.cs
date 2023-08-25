using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class AIHearingComponent : Area3D
{
	[GlobalClass]
	public partial class SoundSource : GodotObject
	{
		public CharacterBase Character;
		public Vector3 Position;
		public double Loudness;
	}

	[Signal]
	public delegate void SoundHeardEventHandler(SoundSource source);

	public void HearSound(SoundSource source)
	{
		Dev.UI.PushDevNotification("Ai sound heard!");
		EmitSignal(SignalName.SoundHeard, source);
	}
}
