using Godot;
using System.Threading.Tasks;

using ProjectMina.EnvironmentQuerySystem;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class RunEnvironmentQuery : Action
{
    private Query _query;
    private int _queryRequestId = 0;
    
    protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
    {
        if (Engine.IsEditorHint())
        {
            return ActionStatus.Failed;
        }

        if (_query == null)
        {
            Fail();
            return Status;
        }
        
        GD.Print("REQ IS TICKING");

        Vector3 bestPosition = await _query.RunQuery(agent);

        if (BlackboardKey != null)
        {
            blackboard.SetValue(BlackboardKey, bestPosition);
        }
        
        Succeed();
        return Status;
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
        
        _query = GetChild<Query>(0);
        
        if (_query is null)
        {
            return;
        }

        GD.Print("query is set up");
    }
    
    public override string[] _GetConfigurationWarnings()
    {
        Godot.Collections.Array<string> warnings = new();
		
        if (GetChildCount() == 0 || GetChildCount() > 1 || GetChild<Query>(0) == null)
        {
            warnings.Add("A RunEnvironmentQuery node must have precisely one child Query node.");
        }

        string[] baseWarnings = base._GetConfigurationWarnings();
        if (baseWarnings != null && baseWarnings.Length > 0)
        {
            warnings.AddRange(baseWarnings);
        }

        string[] errs = new string[warnings.Count];

        for (int i = 0; i < warnings.Count; i++)
        {
            errs.SetValue(warnings[i], i);
        }

        return errs;
    }
}
