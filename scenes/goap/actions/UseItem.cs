using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class UseItem : GoapActionBase
{
    public override bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        switch (primaryGoal.GoalName)
        {
            case "current_health":
                return true;
            case "has_weapon":
                return true;
            default:
                return false;
        }
    }
    
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        GD.Print("target movement position reached? ", (bool)agent.Blackboard.GetValue("target_movement_position_reached"));
        GD.Print("running action Use Item, Primary Goal: ", primaryGoal.GoalName);
        switch (primaryGoal.GoalName)
        {
            case "current_health":
                agent.Blackboard.SetValue("current_health", 100.0f);
                GD.Print("character healed");
                break; 
            case "has_weapon":
                agent.Blackboard.SetValue("has_weapon", true);
                GD.Print("character got weapon");
                break;
            default:
                return ActionStatus.Running;
        }

        agent.Blackboard.SetValue("target_movement_position_reached", false);
        agent.Blackboard.SetValue("target_movement_position", new Vector3());
        agent.Blackboard.SetValue("has_target_movement_position", false);
        
        Status = ActionStatus.Succeeded;
        return Status;
    }
}
