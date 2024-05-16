using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class GoapPlannerSettings : Resource
{
    [Export] public bool EnableDebug { get; protected set; } = false;
    [Export] public int MaxPlanSteps { get; protected set; } = 5;
}