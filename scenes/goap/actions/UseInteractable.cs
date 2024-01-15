using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class UseInteractable : GoapActionBase
{

    private bool _animPlaying = false;
    private bool _interactionStarted = false;
    
    public override Dictionary<StringName, Variant> GetEffects(GoapAgentComponent agent, GoapGoalBase primaryGoal,
        Dictionary<StringName, Variant> worldState)
    {
        Dictionary<StringName, Variant> effect = new();
        switch (primaryGoal.GoalName)
        {
            case "current_health":
                effect.Add("current_health", 100.0f);
                break;
            default:
                break;
        }

        return effect;
    }
    
    public override bool IsValid(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        switch (primaryGoal.GoalName)
        {
            case "current_health":
                return true;
            default:
                return false;
        }
    }
    
    public override ActionStatus Run(GoapAgentComponent agent, GoapGoalBase primaryGoal, Dictionary<StringName, Variant> worldState)
    {
        
        if (_animPlaying)
        {
            return Status = ActionStatus.Running;
        }
        
        if (_interactionStarted)
        {
            agent.Blackboard.SetValue("target_movement_position_reached", false);
            agent.Blackboard.SetValue("target_movement_position", new Vector3());
            agent.Blackboard.SetValue("has_target_movement_position", false);
            agent.Blackboard.SetValue("current_focus", default);
            return Status = ActionStatus.Succeeded;
        }

        Node3D focus = (Node3D)agent.Blackboard.GetValue("current_focus");
        InteractableComponent interactable = focus.GetNode<InteractableComponent>("Interactable");
        
        if (interactable != null)
        {
            AnimationTree animTree = agent.Pawn.CharacterAnimationTree;
            AnimationNodeBlendTree treeRoot = (AnimationNodeBlendTree)animTree.TreeRoot;
            AnimationNodeAnimation oneShotSlot = (AnimationNodeAnimation)treeRoot.GetNode("root_one_shot_slot");
            
            StringName interactionAnim = interactable.ThirdPersonInteractionAnimName;
            oneShotSlot.Animation = interactionAnim;
            animTree.Set("parameters/root_one_shot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            _interactionStarted = true;
            _animPlaying = true;
            
            animTree.AnimationFinished += (StringName animationName) =>
            {
                if (animationName == "interactable/push_button")
                {
                    _animPlaying = false;
                }
            };
        }
        else
        {
            Status = ActionStatus.Failed;
        }
        
        return Status;
    }
}
