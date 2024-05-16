using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class AttackEnemy : ActionBase
{
    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        return EActionStatus.Running;
    }
}
