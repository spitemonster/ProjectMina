using Godot;
using System.Collections.Generic;

namespace ProjectMina.Goap;

public enum GoapPlanStatus : uint
{
    Ready,
    Running,
    Completed,
    Canceled
}
public partial class GoapPlan : GodotObject
{
    public int ID;
    public Queue<GoapActionBase> Steps;
    public GoapGoalBase Goal;
    public GoapPlanStatus Status;
}
