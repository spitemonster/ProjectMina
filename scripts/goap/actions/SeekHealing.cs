using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;

public partial class SeekHealing : GoapActionBase
{
    public override ActionStatus Run(GoapAgentComponent agent, Dictionary<StringName, Variant> worldState)
    {

        if (agent.Healing != null)
        {
            GD.Print("actor sought healing and found it");
            agent.Blackboard.SetValue("healing_located", true);
            agent.Blackboard.SetValue("healing_location", agent.Healing.GlobalPosition);
            return ActionStatus.Succeeded;
        }
        
        return ActionStatus.Running;
    }
    
    public override Dictionary<StringName, Variant> GetPreconditions()
    {
        return Preconditions;
    }

    public override Dictionary<StringName, Variant> GetEffects()
    {
        return Effects;
    }
}
