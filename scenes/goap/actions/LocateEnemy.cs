using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class LocateEnemy : ActionBase
{
    // public virtual bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal,
    //     Dictionary<StringName, Variant> worldState)
    // {
    //     // return false;
    // }
    
    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal,
        Dictionary<StringName, int> worldState)
    {
        GD.Print("[color=yellow]ACTION: ", Name, " running.[/color]");
        agent.Blackboard.SetValue("enemy_visible", true);
        return EActionStatus.Succeeded;
    }
}
