
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;
namespace ProjectMina.EQS;

public enum EQueryTestType : int
{
    Filter,
    Score,
    Both
}

public struct QuerierInfo
{
    public Vector3 QuerierPosition;
}

[Tool]
[GlobalClass]
public partial class Test : EQSNode
{
    
    [Export] public EQueryTestType TestType = EQueryTestType.Both;

    // exclusive min -- must be ABOVE 0.0f
    [Export] public float MinScore = 0.0f;
    
    // inclusive max -- up to and INCLUDING 1.0f
    [Export] public float MaxScore = 1.0f;

    [Signal] public delegate void TestCompletedEventHandler();
    
    // this is the function to override to calculate a point's score
    protected virtual float TestPoint(QuerierInfo querierInfo, QueryPoint point)
    {
        return 1.0f;
    }
    
    // receives an array of items and returns the same sorted, filtered or both
    public virtual async Task<Array<QueryPoint>> RunTest(AIControllerComponent controller, Array<QueryPoint> queryPoints)
    {
        var info = new QuerierInfo()
        {
            QuerierPosition = controller.Pawn.GlobalPosition
        };
        
        return await Task.Run(() =>
        {
            Array<QueryPoint> testedPoints = new();

            foreach (var queryPoint in queryPoints)
            {
                var point = queryPoint;
                
                var score = TestPoint(info, point);

                if (TestType is EQueryTestType.Score or EQueryTestType.Both)
                {
                    point.AddScore(score);
                }

                if (TestType is EQueryTestType.Filter or EQueryTestType.Both)
                {
                    point.AddScore(-point.TestCount - 1);
                }

                testedPoints.Add(point);
            }

            return testedPoints;
        });
    }
}