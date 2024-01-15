using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class LocateEnemy : GoapActionBase
{
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal,
        Dictionary<StringName, Variant> worldState)
    {
        agent.Blackboard.SetValue("enemy_visible", true);
        return ActionStatus.Succeeded;
    }
}
