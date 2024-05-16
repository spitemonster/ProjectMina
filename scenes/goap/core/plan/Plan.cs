using Godot;
using System.Collections.Generic;

namespace ProjectMina.Goap;

public partial class Plan : GodotObject
{
    [Signal] public delegate void PlanStartedEventHandler(GoalBase goal);
    [Signal] public delegate void PlanCanceledEventHandler(GoalBase goal);
    [Signal] public delegate void PlanFailedEventHandler(GoalBase goal, ActionBase failedAction);
    [Signal] public delegate void PlanCompletedEventHandler(GoalBase goal);
    
    public int ID;
    public Queue<ActionBase> Steps;
    public int TotalSteps;
    public GoalBase Goal;
    public EPlanStatus Status { get; protected set; } = EPlanStatus.Ready;

    public ActionBase GetNextAction()
    {
        return Steps.Dequeue();
    }

    public void Start()
    {
        Status = EPlanStatus.Running;
    }
    
    public void Cancel()
    {
        Status = EPlanStatus.Canceled;
        Free();
    }
    
    public void Fail()
    {
        Status = EPlanStatus.Failed;
    }

    public void Succeed()
    {
        Status = EPlanStatus.Succeeded;
    }
    
    public void Complete()
    {
        Free();
    }
}
