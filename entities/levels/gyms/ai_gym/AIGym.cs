using Godot;
using Godot.Collections;

namespace ProjectMina;
[GlobalClass]
public partial class AIGym : LevelBase
{

	[Export] protected Node3D TestFocus;
	
	private Array<AICharacter> _aiCharacters = new();
	public override void _Ready()
	{
		base._Ready();

		var nodes = GetTree().GetNodesInGroup("ai_characters");

		foreach (var node in nodes)
		{
			if (node is AICharacter c)
			{
				_aiCharacters.Add(c);
			}
		}
		
		GD.Print("ai characters: ", _aiCharacters);

		if (TestFocus != null)
		{
			foreach (var character in _aiCharacters)
			{
				character.AIController.SetFocus(TestFocus);
			}	
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		DebugDraw.Box(new Vector3(0,0,0), new Vector3(1,1,1), Colors.Burlywood);
	}
}
