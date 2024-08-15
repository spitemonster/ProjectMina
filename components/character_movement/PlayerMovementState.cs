using Godot;

namespace ProjectMina;
public partial class PlayerMovementState : CharacterMovementState
{
    protected PlayerCharacter Player;
    protected AnimationNodeStateMachinePlayback PositionAnimStateMachine;
    protected AnimationNodeTimeScale MovementAnimTimeScale;
    public override bool Setup(CharacterBase character)
    {
        
            
        if (Character != null || character is not PlayerCharacter)
        {
            GD.PushError("character is not player");
            return false;
        }

        if (!base.Setup(character))
        {
            return false;
        }
        
        Player = (PlayerCharacter)Character;

        return true;
    }

    public override void SetupAnimComponents()
    {
        base.SetupAnimComponents();
        var smRoot = (AnimationNodeBlendTree)AnimTreeRoot;
        AnimStateMachineRoot = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/movement_anims/playback");
        PositionAnimStateMachine = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/position_anims/playback");
        // MovementAnimTimeScale = (AnimationNodeTimeScale)smRoot.GetNode("movement_speed_scale");
    }
}
