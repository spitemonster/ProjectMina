using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class Idle : GoalBase
{
    public override double Priority(Dictionary<StringName, int> worldState)
    {
        return BasePriority;
    }
}
