using Godot;
using System;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class IsSuspicious : Decorator
{
    public override bool Run(AIControllerComponent controller, BlackboardComponent blackboard)
    {
        return blackboard.ValueEqual("state", "suspicious");
    }
}
