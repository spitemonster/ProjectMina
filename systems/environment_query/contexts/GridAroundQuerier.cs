using System.Collections.Generic;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EQS;

[Tool]
[GlobalClass]
public partial class GridAroundQuerier : Context
{

    [Export] public Vector3 HalfExtent = new Vector3(5, 5, 5);
    [Export] public float GridSpacing = 1.0f;
    
    private Array<QueryPoint> _points = new();

    [Signal] public delegate void PointsCreatedEventHandler();
    
    public override async Task<Array<QueryPoint>> GeneratePoints(AIControllerComponent querier)
    {
        _points = new();
        CallDeferred("_CreatePoints", querier);
        await ToSignal(this, SignalName.PointsCreated);
        return _points;
    }
    

    private void _CreatePoints(AIControllerComponent querier)
    {
        var origin = querier.Pawn.GlobalPosition + new Vector3(0, .25f, 0);

        for (float x = -HalfExtent.X; x <= HalfExtent.X; x += GridSpacing)
        {
            for (float z = -HalfExtent.Z; z <= HalfExtent.Z; z += GridSpacing)
            {
                var pos = new Vector3(x + origin.X, origin.Y, z + origin.Z);
                var point = new QueryPoint()
                {
                    GlobalPosition = pos,
                    TotalScore = 0
                };
                
                _points.Add(point);
            }
        }

        EmitSignal(SignalName.PointsCreated);
    }
}
