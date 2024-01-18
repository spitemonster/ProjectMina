using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class LocateEnemy : GoapActionBase
{
    // public virtual bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal,
    //     Dictionary<StringName, Variant> worldState)
    // {
    //     // return false;
    // }
    
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal,
        Dictionary<StringName, Variant> worldState)
    {
        GD.Print("[color=yellow]ACTION: ", Name, " running.[/color]");
        agent.Blackboard.SetValue("enemy_visible", true);
        return ActionStatus.Succeeded;
    }
}
