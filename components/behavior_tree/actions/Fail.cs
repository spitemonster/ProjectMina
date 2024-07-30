using Godot;

namespace ProjectMina.BehaviorTree;

[GlobalClass]
public partial class Fail : Action
{
    protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
    {
        SetStatus(EActionStatus.Failed);
        return Status;
    }
}
