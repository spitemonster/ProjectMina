using System.Linq;
using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

[GlobalClass]
public partial class LocateInteractable : ActionBase
{
    public override EActionStatus Run(AgentComponent agent, GoalBase primaryGoal, Dictionary<StringName, int> worldState)
    {
        
        var interactable = _LocateInteractable(agent, primaryGoal);

        if (interactable != null)
        {
            agent.Blackboard.SetValue("has_target_movement_position", true);
            agent.Blackboard.SetValue("current_focus", interactable.GetOwner<Node3D>());
            agent.Blackboard.SetValue("target_movement_position_reached", false);
            
            if (interactable.ActionPosition != null && interactable.ActionPosition.GlobalPosition != Vector3.Zero)
            {
                agent.Blackboard.SetValue("target_movement_position", interactable.ActionPosition.GlobalPosition);
            }
            else
            {
                agent.Blackboard.SetValue("target_movement_position", interactable.GetOwner<Node3D>().GlobalPosition);
            }
            
            Status = EActionStatus.Succeeded;
        }
        else
        {
            Status = EActionStatus.Running;
        }

        return Status;
    }

    private static InteractableComponent _LocateInteractable(AgentComponent agent, GoalBase primaryGoal)
    {
        var nearbyNodes = agent.Pawn.SearchComponent.SearchArea.GetOverlappingBodies();
        
        foreach (var node in nearbyNodes)
        {
            GD.PrintRich("[color=orange]Locate Interactable node name: ", node.Name, "[/color]");
            InteractableComponent i = node.GetNodeOrNull<InteractableComponent>("Interactable") ?? node.GetNodeOrNull<EquippableComponent>("Equippable") ?? node.GetNodeOrNull<WeaponComponent>("Weapon");

            if (i == null)
            {
                continue;
            }
            
            GD.PrintRich("[color=orange]Locate Interactable component: ", i.Name, "[/color]");

            if (!i.UseState.ContainsKey(primaryGoal.GoalName))
            {
                GD.PrintRich("[color=orange]Locate Interactable component useState does not contain : ", primaryGoal.GoalName, "[/color]");
            }

            if (i.UseState.ContainsKey(primaryGoal.GoalName)
                && i.UseState[primaryGoal.GoalName].Equals(primaryGoal.BaseDesiredValue)
                && node != agent.Pawn
                && agent.Pawn.HasLineOfSight(node))
            {
                return i;
            }
        }

        return null;
    }

    private static InteractableComponent _LocateHealing(AgentComponent agent)
    {
        return null;
    }
}
