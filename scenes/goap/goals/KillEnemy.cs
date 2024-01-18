using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class KillEnemy : GoapGoalBase
{
    public override double Priority(Dictionary<StringName, Variant> worldState)
    {
        // this goal shouldn't really have priority if we have no enemy and we don't have a weapon
        var hasWeapon = (bool)worldState["has_weapon"];
        var enemyVisible = (bool)worldState["enemy_visible"];
        
        if (!hasWeapon || !enemyVisible)
        {
            return 0;
        }

        return BasePriority;
    }
}
