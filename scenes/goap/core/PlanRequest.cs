using Godot;

namespace ProjectMina.Goap;
partial class PlanRequest: GodotObject
{
    public int ID;
    public GoapAgentComponent Agent;
    public GoapGoalBase Goal;

    public void Complete()
    {
        Free();
    }
}