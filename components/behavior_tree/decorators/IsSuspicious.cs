using Godot;
using System;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class IsSuspicious : Decorator
{
    public override bool Run(AIControllerComponent controller, BlackboardComponent blackboard)
    {
        GD.Print("running decorator");
        return blackboard.ValueEqual("current_state", "suspicious");
    }
}
