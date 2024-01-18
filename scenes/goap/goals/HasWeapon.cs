using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class HasWeapon : GoapGoalBase
{
    public override double Priority(Dictionary<StringName, Variant> worldState)
    {
        if ((bool)worldState["has_weapon"])
        {
            return 0;
        }
        
        GD.PrintRich("[color=green]testing priority of has weapon goal[/color]");
        GD.PrintRich("[color=green]current health: ", (double)worldState["current_health"], "[/color]");
        GD.PrintRich("[color=green]max health: ", (double)worldState["max_health"], "[/color]");

        return (double)worldState["current_health"] / (double)worldState["max_health"];
    }
}
