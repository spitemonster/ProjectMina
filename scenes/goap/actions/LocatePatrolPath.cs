using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;
[GlobalClass]
public partial class LocatePatrolPath : ActionBase
{
    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        GD.Print("RUNNING ACTION LOCATE PATROL PATH");
        var patrolPath = (PatrolPath)Global.Data.CurrentLevel.GetNode("%PatrolPath");
        GD.Print("HERE IS PATROL PATH: ", patrolPath);

        if (patrolPath != null)
        {
            agent.Blackboard.SetValue("has_patrol_path", true);
            agent.Blackboard.SetValue("current_patrol_path", patrolPath);
            Status = EActionStatus.Succeeded;
        }
        else
        {
            Status = EActionStatus.Failed;
        }

        return Status;
    }
}
