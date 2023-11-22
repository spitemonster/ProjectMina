using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class UseHealing : GoapActionBase
{
    public override bool IsValid(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
    {
        var blackboard = agent.Blackboard;
        if (!blackboard.HasValue("healing_location") || !blackboard.HasValue("target_movement_position") ||
            !blackboard.HasValue("target_movement_position_reached"))
        {
            return false;
        }

        if (blackboard.GetValue("healing_location").Equals(blackboard.GetValue("target_movement_position")) &&
            (bool)blackboard.GetValue("target_movement_position_reached"))
        {
            return true;
        }

        return false;
    }

    public override ActionStatus Run(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
    {
        agent.Blackboard.SetValue("current_health", 100.0f);
        agent.Blackboard.SetValue("healing_located", false);
        GD.Print("character healed");
        return ActionStatus.Succeeded;
    }
}
