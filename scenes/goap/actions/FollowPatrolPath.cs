using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class FollowPatrolPath : GoapActionBase
{

    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        var patrolPath = (PatrolPath)agent.Blackboard.GetValue("current_patrol_path");
        var currentPatrolPoint = (int)agent.Blackboard.GetValue("current_patrol_path_point");
        var currentPatrolPathPointPosition = patrolPath.GetPatrolPointPosition(currentPatrolPoint);

        if (agent.Pawn.NavigationAgent.IsTargetReached())
        {
            if (agent.Pawn.NavigationAgent.TargetPosition == currentPatrolPathPointPosition)
            {
                if (currentPatrolPoint < patrolPath.GetPatrolPointsCount() - 1)
                {
                    currentPatrolPoint++;
                    Status = ActionStatus.Running;
                }
                else
                {
                    currentPatrolPoint = 0;
                    Status = ActionStatus.Succeeded;
                }
                
                agent.Blackboard.SetValue("current_patrol_path_point", currentPatrolPoint);
                
            }
            else
            {
                agent.Blackboard.SetValue("target_movement_position", currentPatrolPathPointPosition);
                agent.Pawn.NavigationAgent.TargetPosition = currentPatrolPathPointPosition;
                Status = ActionStatus.Running;
            }
        }
        else if (agent.Pawn.NavigationAgent.TargetPosition == currentPatrolPathPointPosition)
        {
            Status = ActionStatus.Running;
        }
        else
        {
            agent.Blackboard.SetValue("target_movement_position", currentPatrolPathPointPosition);
            agent.Pawn.NavigationAgent.TargetPosition = currentPatrolPathPointPosition;
            Status = ActionStatus.Running;
        }

        return Status;
    }
}
