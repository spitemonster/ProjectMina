using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class FollowPatrolPath : ActionBase
{

    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
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
                    Status = EActionStatus.Running;
                }
                else
                {
                    currentPatrolPoint = 0;
                    Status = EActionStatus.Succeeded;
                }
                
                agent.Blackboard.SetValue("current_patrol_path_point", currentPatrolPoint);
                
            }
            else
            {
                agent.Blackboard.SetValue("target_movement_position", currentPatrolPathPointPosition);
                agent.Pawn.NavigationAgent.TargetPosition = currentPatrolPathPointPosition;
                Status = EActionStatus.Running;
            }
        }
        else if (agent.Pawn.NavigationAgent.TargetPosition == currentPatrolPathPointPosition)
        {
            Status = EActionStatus.Running;
        }
        else
        {
            agent.Blackboard.SetValue("target_movement_position", currentPatrolPathPointPosition);
            agent.Pawn.NavigationAgent.TargetPosition = currentPatrolPathPointPosition;
            Status = EActionStatus.Running;
        }

        return Status;
    }
}
