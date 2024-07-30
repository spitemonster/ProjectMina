using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Godot;
using System.Threading.Tasks;
using Godot.Collections;
using ProjectMina.EnvironmentQuerySystem;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class RunEnvironmentQuery : Action
{
    private Query _query;
    private int _queryRequestId = 0;
    
    protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
    {
        if (Engine.IsEditorHint())
        {
            return EActionStatus.Failed;
        }

        if (_query == null)
        {
            SetStatus(EActionStatus.Failed);
            return Status;
        }
        
        // Dictionary<Vector3, float> testedPoints = await _query.RunQuery(controller);
        //
        // if (_debug)
        // {
        //     
        //     foreach (var (key, value) in testedPoints)
        //     {
        //         var color = value switch
        //         {
        //             > .99f => Colors.Green,
        //             > .5f => Colors.Yellow,
        //             > 0 => Colors.Red,
        //             _ => Colors.Black
        //         };
        //     
        //         DebugDraw.Sphere(key, 1, color, 5f);
        //     }
        // }
        //
        //
        //
        // if (testedPoints.Values.First() < 0)
        // {
        //     Fail();
        //     return Status;
        // }
        //
        // if (BlackboardKey != null)
        // {
        //     blackboard.SetValue(BlackboardKey, testedPoints.Keys.First());
        // }
        
        SetStatus(EActionStatus.Succeeded);
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
