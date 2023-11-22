using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MoveToTargetPosition : GoapActionBase
{
    public override bool IsValid(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
    {
        // don't run if we don't have the key
        if (!worldState.ContainsKey("target_position"))
        {
            return false;
        }

        // don't run if the target position isn't a vector
        if (worldState["target_position"].VariantType != Variant.Type.Vector3)
        {
            return false;
        }

        // return if the target position is a valid position
        return (Vector3)worldState["target_position"] != new Vector3();
    }

    public override ActionStatus Run(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
    {
        if (agent.Pawn.Brain.NavigationAgent.IsTargetReached())
        {
            return ActionStatus.Succeeded;
        }
        else
        {
            GD.Print("character should move to target position");
            agent.Pawn.Brain.NavigationAgent.TargetPosition = (Vector3)worldState["target_position"];
        }
        return ActionStatus.Running;
    }
} 