using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MaximizeHealth : GoapGoalBase
{
    public override double Priority(Dictionary<StringName, Variant> worldState)
    {
        return (double)worldState["max_health"] - (double)worldState["current_health"];
    }
}
