using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;
[GlobalClass]
public partial class LocatePatrolPath : GoapActionBase
{
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        GD.Print("RUNNING ACTION LOCATE PATROL PATH");
        var patrolPath = (PatrolPath)Global.Data.CurrentLevel.GetNode("%PatrolPath");
        GD.Print("HERE IS PATROL PATH: ", patrolPath);

        if (patrolPath != null)
        {
            agent.Blackboard.SetValue("has_patrol_path", true);
            agent.Blackboard.SetValue("current_patrol_path", patrolPath);
            Status = ActionStatus.Succeeded;
        }
        else
        {
            Status = ActionStatus.Failed;
        }

        return Status;
    }
}
