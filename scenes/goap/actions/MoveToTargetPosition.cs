using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MoveToTargetPosition : GoapActionBase
{
    public override bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        // don't run if we don't have the key
        if (!worldState.ContainsKey("target_movement_position"))
        {
            GD.Print("world state doesn't contain target movement position key");
            return false;
        }

        // don't run if the target position isn't a vector
        if (worldState["target_movement_position"].VariantType != Variant.Type.Vector3)
        {
            GD.Print("target movement position isn't a vector");
            return false;
        }

        return true;
        // we need to do some extra work here because there are technically situations where this is not valid to use but for now this is fine
    }

    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        agent.Pawn.Brain.NavigationAgent.TargetPosition = (Vector3)worldState["target_movement_position"];
        // GD.Print("target movement position: ", (Vector3)worldState["target_movement_position"]);
        
        if (agent.Pawn.Brain.NavigationAgent.IsTargetReached())
        {
            GD.Print("reached");
            agent.Blackboard.SetValue("target_movement_position_reached", true);
            agent.Blackboard.SetValue("target_movement_position", Vector3.Zero);
            Status = ActionStatus.Succeeded;
        }
        else
        {
            Status = ActionStatus.Running;
        }

        return Status;
    }
} 