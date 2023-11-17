using Godot;
using Godot.Collections;
using System.Diagnostics;

namespace ProjectMina.Goap;
[Tool]
[GlobalClass]
public partial class Agent : ComponentBase
{
	[Export] public BlackboardComponent Blackboard { get; protected set; }

	public override void _Ready()
	{
		System.Diagnostics.Debug.Assert(Blackboard != null, "Agent does not have Blackboard");
		System.Diagnostics.Debug.Assert(Planner.Instance != null, "Agent does not have access to the Planner Instance");

		Blackboard.ValueChanged += (string key, Variant newValue) =>
		{

		};
	}

	public override void _Process(double delta)
	{
	}
}
