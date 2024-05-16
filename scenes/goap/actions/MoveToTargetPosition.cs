using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MoveToTargetPosition : ActionBase
{
    public override bool IsValid(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        // don't run if we don't have the key
        if (!worldState.ContainsKey("target_movement_position"))
        {
            return false;
        }
        //
        // // don't run if the target position isn't a vector
        // if (worldState["target_movement_position"].VariantType != Variant.Type.Vector3)
        // {
        //     return false;
        // }

        return true;
        // we need to do some extra work here because there are technically situations where this is not valid to use but for now this is fine
    }

    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        Status = EActionStatus.Running;
        // if (agent.Pawn.NavigationAgent.IsTargetReached())
        // {
        //     if ((Vector3)worldState["target_movement_position"] == agent.Pawn.NavigationAgent.TargetPosition)
        //     {
        //         agent.Blackboard.SetValue("target_movement_position_reached", true);
        //         agent.Blackboard.SetValue("target_movement_position", Vector3.Zero);
        //         Status = EActionStatus.Succeeded;
        //     }
        //     else
        //     {
        //         agent.Pawn.NavigationAgent.TargetPosition = (Vector3)worldState["target_movement_position"];
        //         Status = EActionStatus.Running;    
        //     }
        // } else if (agent.Pawn.NavigationAgent.TargetPosition != (Vector3)worldState["target_movement_position"])
        // {
        //     agent.Pawn.NavigationAgent.TargetPosition = (Vector3)worldState["target_movement_position"];
        //     Status = EActionStatus.Running;
        // }
        // else
        // {
        //     
        // }
        //
        return Status;
    }
} 