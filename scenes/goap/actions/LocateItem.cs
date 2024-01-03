using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class LocateItem : GoapActionBase
{
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        switch (primaryGoal.GoalName)
        {
            case "current_health": 
                Status = _LocateHealth(agent);
                break;
            case "has_weapon":
                Status = _LocateWeapon(agent);
                break;
            default:
                Status = ActionStatus.Running;
                break;
        }
        
        return Status;
    }

    private ActionStatus _LocateHealth(GoapAgentComponent agent)
    {
        if (agent.Healing != null)
        {
            agent.Blackboard.SetValue("target_movement_position_reached", false);
            agent.Blackboard.SetValue("has_target_movement_position", true);
            agent.Blackboard.SetValue("target_movement_position", agent.Healing.GlobalPosition);
            GD.Print("set target movement position");
            return ActionStatus.Succeeded;
        }
        else
        {
            return ActionStatus.Running;    
        }
    }

    private ActionStatus _LocateWeapon(GoapAgentComponent agent)
    {
        if (agent.Weapon != null)
        {
            agent.Blackboard.SetValue("target_movement_position_reached", false);
            agent.Blackboard.SetValue("has_target_movement_position", true);
            GD.Print("WEAPON POSITION: ", agent.Weapon.GlobalPosition);
            GD.Print("CHARACTER POSITION: ", agent.Pawn.GlobalPosition);
            agent.Blackboard.SetValue("target_movement_position", agent.Weapon.GlobalPosition);
            
            GD.Print("set target movement position");
            return ActionStatus.Succeeded;
        }
        else
        {
            return ActionStatus.Running;    
        }
    }
}
