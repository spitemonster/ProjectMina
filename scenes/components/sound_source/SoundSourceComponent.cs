using Godot;

namespace ProjectMina;
public partial class SoundSourceComponent : Area3D
{
	public void TriggerSound(Vector3 position = new(), float volume = 1.0f)
	{
		Godot.Collections.Array<Area3D> overlappingAreas = GetOverlappingAreas();

		foreach (var overlappingArea in overlappingAreas)
		{
			// if (overlappingArea is AIPerceptionComponent)	
		}
	}
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
