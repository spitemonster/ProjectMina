using Godot;

namespace ProjectMina;

public enum EStimulusType : uint
{
	None = 0,
	HearingDynamicFootstep,
	HearingDynamicCrash,
	Sight
}

[GlobalClass]
public partial class AISenseStimulusComponent : Area3D
{
	
	public override void _Ready()
	{
	}
	
	public override void _Process(double delta)
	{
	}
}
