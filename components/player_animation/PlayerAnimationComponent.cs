using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class PlayerAnimationComponent : CharacterAnimationComponent
{
    [Signal] public delegate void RightHandStateUpdatedEventHandler(StringName stateName);
    
    protected PlayerCharacter Player;
    private AnimationNodeBlendTree _rightHandCurrentState;
    private AnimationNodeStateMachine _rightHandStateMachine;
    private AnimationNodeStateMachinePlayback _rightHandStateMachinePlayback;
    private StringName _rightHandCurrentStateName;

    public void PlayAttack()
    {
        GD.Print("should attack!");
        GD.Print($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_attack/request");
        AnimTree.Set($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_attack/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
        AnimTree.Set($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        // var attackOneShot = (AnimationNodeOneShot)_rightHandCurrentState.GetNode("os_attack");
        //
        // if (attackOneShot == null)
        // {
        //     GD.Print("double shit!");
        // }
        //
        // attackOneShot?.SetParameter("request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    public void PlayEquip()
    {
        GD.Print("should equip");
        GD.Print($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_equip/request");
        AnimTree.Set($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_equip/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    // assuming the player has a ranged weapon equipped, reload it
    public void PlayReload()
    {
        if (_rightHandCurrentState.HasNode("os_reload"))
        {
            AnimTree.Set($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_reload/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);    
        }
        else
        {
            GD.Print("no reload anim!");
        }
        
    }

    public void PlayWeaponEmpty()
    {
        if (_rightHandCurrentState.HasNode("os_empty"))
        {
            AnimTree.Set($"parameters/sm_right_hand/{_rightHandCurrentStateName}/os_empty/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);    
        }
        else
        {
            GD.Print("no weapon empty anim");
        }
    }

    public override void _Ready()
    {
        base._Ready();

        Player = GetOwner<PlayerCharacter>();

        if (Player == null)
        {
            GD.PushError("PlayerAnimComponent not attached to player character");
            return;
        }

        if (AnimTree == null)
        {
            GD.PushError("PlayerAnimComponent has no anim tree");
            return;
        }

        if (AnimTreeRoot == null)
        {
            GD.PushError("Anim tree root is null");
            return;
        }
        
        Player.WeaponEquipped += _ChangeWeaponAnim;
        Player.RangedWeaponFired += _PlayRangedAttackAnim;
        
        _rightHandStateMachine = (AnimationNodeStateMachine)AnimTreeRoot.GetNode("sm_right_hand");
        _rightHandStateMachinePlayback = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/sm_right_hand/playback");
        _rightHandCurrentStateName = _rightHandStateMachinePlayback.GetCurrentNode();
    }

    private void _PlayRangedAttackAnim(RangedWeaponComponent rangedWeapon)
    {
        PlayAttack();
    }

    public override void _Process(double delta)
    {
        // if (_rightHandStateMachinePlayback.GetCurrentNode() != _rightHandCurrentStateName)
        // {
        //     // _UpdateRightHandCurrentState();
        // }
    }

    private void _UpdateRightHandCurrentState()
    {
        _rightHandCurrentStateName = _rightHandStateMachinePlayback.GetCurrentNode();
        var currentNode = $"parameters/sm_right_hand/playback/{_rightHandCurrentStateName}";
        _rightHandCurrentState = (AnimationNodeBlendTree)_rightHandStateMachine.GetNode(_rightHandCurrentStateName);
        
        GD.Print(currentNode);

        if (_rightHandCurrentState != null)
        {
            GD.Print("has attack: ", _rightHandCurrentState.HasNode("os_attack"));
        }
        else
        {
            GD.Print("shit");
        }
        
        EmitSignal(SignalName.RightHandStateUpdated);
    }

    private void _ChangeWeaponAnim(EWeaponType weaponType)
    {
        switch (weaponType)
        {
            case EWeaponType.Melee:
                _rightHandStateMachinePlayback.Travel("melee");
                break;
            case EWeaponType.Ranged:
                _rightHandStateMachinePlayback.Travel("ranged");
                break;
            case EWeaponType.None:
                _rightHandStateMachinePlayback.Travel("empty");
                break;
            default:
                break;
        }


        CallDeferred("_UpdateRightHandCurrentState");
        CallDeferred("PlayEquip");
    }
}