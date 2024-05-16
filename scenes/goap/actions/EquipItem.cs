using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class EquipItem : ActionBase
{
    public override Dictionary<StringName, int> GetEffects(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        Dictionary<StringName, int> effect = new();
        switch (primaryGoal.GoalName)
        {
            case "has_weapon":
                effect.Add("has_weapon", 1);
                break;
            case "has_equipment":
                effect.Add("has_equipment", 1);
                break;
        }

        return effect;
    }
    
    public override bool IsValid(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
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

    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
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
                            case EWeaponRange.Melee:
                                agent.Blackboard.SetValue("has_melee_weapon", true);
                                break;
                            case EWeaponRange.Ranged:
                                agent.Blackboard.SetValue("has_ranged_weapon", true);
                                break;
                        }
                    }
                    
                    break;
            }
            
            Status = EActionStatus.Succeeded;
        }
        else
        {
            Status = EActionStatus.Failed;
        }

        return Status;
    }
}
