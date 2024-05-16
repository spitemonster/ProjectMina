using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MaximizeHealth : GoalBase
{
    public override double Priority(Dictionary<StringName, int> worldState)
    {
        return (double)worldState["max_health"] - (double)worldState["current_health"];
    }
}
