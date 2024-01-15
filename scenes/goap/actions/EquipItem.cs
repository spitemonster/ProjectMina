using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class EquipItem : GoapActionBase
{
    public override Dictionary<StringName, Variant> GetEffects(GoapAgentComponent agent, GoapGoalBase primaryGoal,
        Dictionary<StringName, Variant> worldState)
    {
        Dictionary<StringName, Variant> effect = new();
        switch (primaryGoal.GoalName)
        {
            case "has_weapon":
                effect.Add("has_weapon", true);
                break;
            case "has_equipment":
                effect.Add("has_equipment", true);
                break;
        }

        return effect;
    }
    
    public override bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        switch (primaryGoal.GoalName)
        {
            case "has_weapon":
                return true;
            case "has_equipment":
                return true;
            default:
                return false;
        }
    }

    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        var focus = (Node3D)agent.Blackboard.GetValue("current_focus");
        var equippable = focus.GetNodeOrNull<EquippableComponent>("Equippable") ??
                         focus.GetNodeOrNull<EquippableComponent>("Interactable") ??
                         focus.GetNodeOrNull<WeaponComponent>("Weapon");

        if (equippable != null)
        {
            GD.Print("CHARACTER CAN EQUIP ITEM: ", equippable.Name);
            agent.Pawn.CharacterEquipment.Equip(equippable);

            switch (equippable.Type)
            {
                case EquippableComponent.EquipmentType.Tool:
                    agent.Blackboard.SetValue("has_equipment", true);
                    break;
                case EquippableComponent.EquipmentType.Weapon:
                    agent.Blackboard.SetValue("has_weapon", true);
                    
                    if (equippable is WeaponComponent weapon)
                    {
                        switch (weapon.Range)
                        {
                            case WeaponRange.Melee:
                                agent.Blackboard.SetValue("has_melee_weapon", true);
                                break;
                            case WeaponRange.Ranged:
                                agent.Blackboard.SetValue("has_ranged_weapon", true);
                                break;
                        }
                    }
                    
                    break;
            }
            
            return ActionStatus.Succeeded;
        }
        
        return ActionStatus.Failed;
    }
}
