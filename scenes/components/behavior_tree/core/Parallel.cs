using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class Parallel : Composite
{
    protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
    {
        List<Task<ActionStatus>> childTasks = new List<Task<ActionStatus>>();

        foreach (Action task in GetChildren())
        {
            if (task != null)
            {
                childTasks.Add(task.Tick(agent, blackboard));
            }
        }
        
        // Wait for all child tasks to complete
        var results = await Task.WhenAll(childTasks);

        // Determine the status of the parallel node based on child statuses
        if (results.Any(status => status == ActionStatus.Succeeded))
        {
            Succeed();
        }
        else if (results.All(status => status == ActionStatus.Failed))
        {
            Fail();
        }
        else
        {
            Run();
        }

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