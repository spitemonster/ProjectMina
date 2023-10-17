using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;
[Tool]
[GlobalClass]
public partial class Agent : Node
{
	[Export] public BlackboardAsset Blackboard { get; protected set; }
	public WorldState AgentState = new();

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
