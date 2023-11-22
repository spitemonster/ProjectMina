using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MoveToHealing : GoapActionBase
{
	public override ActionStatus Run(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
	{
		if (agent.Pawn.Brain.NavigationAgent.IsTargetReached())
		{
			return ActionStatus.Succeeded;
		}
		else
		{
			GD.Print("character should move to target position");
			agent.Pawn.Brain.NavigationAgent.TargetPosition = (Vector3)worldState["healing_position"];
		}
		return ActionStatus.Running;
	}
}
