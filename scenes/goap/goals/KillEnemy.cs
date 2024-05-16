using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class KillEnemy : GoalBase
{
    public override double Priority(Dictionary<StringName, int> worldState)
    {
        return BasePriority;
    }
}
