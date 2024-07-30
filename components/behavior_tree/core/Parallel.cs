using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class Parallel : Composite
{
    
    protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
    {

        var primaryAction = ChildActions[0];
        var secondaryAction = ChildActions[1];
        
        secondaryAction.Tick(controller, blackboard);
        
        var mainTaskResult = primaryAction.Tick(controller, blackboard);
        SetStatus(mainTaskResult);

        return Status;
    }
    
    
    public override string[] _GetConfigurationWarnings()
    {
        Godot.Collections.Array<string> warnings = new();
        
        string[] baseWarnings = base._GetConfigurationWarnings();
        if (baseWarnings != null && baseWarnings.Length > 0)
        {
            warnings.AddRange(baseWarnings);
        }

        if (GetChildCount() > 2)
        {
            warnings.Add("A Parallel node cannot have more than two child tasks.");
        }

        foreach (var child in GetChildren())
        {
            if (child is not Action a)
            {
                warnings.Add("All Behavior tree nodes must inherit from Action.");
            }
        }

        string[] errs = new string[warnings.Count];

        for (int i = 0; i < warnings.Count; i++)
        {
            errs.SetValue(warnings[i], i);
        }

        return errs;
    }
}