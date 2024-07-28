using Godot;
using System;
using ProjectMina;

public partial class Game : Node
{
	public static Game Instance;
	
	public override void _EnterTree()
	{
		if (Instance != null)
		{
			QueueFree();
		}
		Instance = this;
	}

	public static void SpawnParticleSystemAtPosition(Vector3 position, EPhysicsMaterialType type = 0)
	{
		
	} 
}
