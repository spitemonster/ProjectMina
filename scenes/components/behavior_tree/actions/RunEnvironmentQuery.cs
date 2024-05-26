using System;
using System.Threading.Tasks;
using Godot;

using ProjectMina.EQS;
namespace ProjectMina.BehaviorTree;
public partial class RunEQSQuery : Action
{
    private Query _query;
    
    protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
    {
        await Task.Run(() =>
        {
            _query.RunQuery(character);
        });
        
        Succeed();

        return Status;
    }

    public override void _Ready()
    {
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
            warnings.Add("A Run EQS Query node must have precisely one child Query node.");
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
