using Godot;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class Decorator : Resource
{
    public virtual bool Run(AIControllerComponent controller, BlackboardComponent blackboard)
    {
        return false;
    }
}
