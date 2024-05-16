using Godot;

namespace ProjectMina.Goap;
partial class PlanRequest: GodotObject
{
    public int ID;
    public AgentComponent Agent;
    public GoalBase Goal;

    public void Fulfill()
    {
        Free();
    }
}