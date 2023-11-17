using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class SoundComponent : ComponentBase
{
	[Export]
	public Area3D TriggerArea;

	// private AIHearingComponent.SoundSource source = new();

	public void EmitSound()
	{
		// if (TriggerArea != null)
		// {
		// 	Godot.Collections.Array<Area3D> overlappingAreas = TriggerArea.GetOverlappingAreas();

		// 	foreach (Node3D area in overlappingAreas)
		// 	{
		// 		if (area is AIHearingComponent h)
		// 		{
		// 			source.Position = GlobalPosition;

		// 			h.HearSound(source);
		// 		}
		// 	}
		// }
	}

	public override void _Ready()
	{
		// if (TriggerArea != null)
		// {
		// 	source.Character = GetOwner<CharacterBase>();
		// }
	}
}
