using Godot;

namespace ProjectMina;

public partial class AIHearingComponent : ComponentBase
{
	// [GlobalClass]
	// public partial class SoundSource : GodotObject
	// {
	// 	public CharacterBase Character;
	// 	public Vector3 Position;
	// 	public double Loudness;
	// }

	// [Signal]
	// public delegate void SoundHeardEventHandler(SoundSource source);

	// public void HearSound(SoundSource source)
	// {
	// GD.Print("Ai sound heard!");
	// 	EmitSignal(SignalName.SoundHeard, source);
	// }

	public override void _Ready()
	{
		base._Ready();
	}
}
