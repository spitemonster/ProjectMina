using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class MaximizeHealth : GoapGoalBase
{
    public override double Priority(Dictionary<StringName, Variant> worldState)
    {
        GD.Print("checking priority of maximize health: ", (double)worldState["max_health"] - (double)worldState["current_health"]);
        GD.Print("max health: ", (double)worldState["max_health"]);
        GD.Print("curren health: ", (double)worldState["current_health"]);
        return (double)worldState["max_health"] - (double)worldState["current_health"];
    }
}
