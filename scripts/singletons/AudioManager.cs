using Godot;
using System;
using ProjectMina;

public partial class AudioManager : Node
{
	public AudioManager Instance;
	
	public override void _EnterTree()
	{
		if (Instance == null)
		{
			QueueFree();
			return;
		}

		Instance = this;
	}

	public void PlaySoundAtPosition(Vector3 position)
	{
	}
}
