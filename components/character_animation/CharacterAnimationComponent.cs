using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class CharacterAnimationComponent : ComponentBase
{
    [Export] public CharacterAnimationTree AnimTree;
    protected CharacterBase Character;
    protected AnimationNodeBlendTree AnimTreeRoot;

    public override void _Ready()
    {
        if (AnimTree == null) return;

        AnimTreeRoot = (AnimationNodeBlendTree)AnimTree.TreeRoot;
    }
}
