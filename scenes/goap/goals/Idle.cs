using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class Idle : GoapGoalBase
{
    public override double Priority(Dictionary<StringName, Variant> worldState)
    {
        if ((bool)worldState["occupied"] == true)
        {
            return 0;
        }

        return BasePriority;
    }
}
