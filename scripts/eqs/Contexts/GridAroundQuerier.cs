using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class GridAroundQuerier : Context
{

    [Export] public Vector3 HalfExtent = new Vector3(5, 5, 5);
    [Export] public float GridSpacing = 1.0f;
    
    private Array<Vector3> _points = new();

    [Signal] public delegate void PointsCreatedEventHandler();
    
    public override async Task<Array<Vector3>> GetPoints(AgentComponent querier)
    {
        _points = new();
        CallDeferred("_CreatePoints", querier);

        await ToSignal(this, SignalName.PointsCreated);
        
        return _points;
    }

    private void _CreatePoints(AgentComponent querier)
    {
        var origin = querier.Pawn.GlobalPosition + new Vector3(0, .5f, 0);

        for (float x = -HalfExtent.X; x <= HalfExtent.X; x += GridSpacing)
        {
            for (float y = -HalfExtent.Y; y <= HalfExtent.Y; y += GridSpacing)
            {

                for (float z = -HalfExtent.Z; z <= HalfExtent.Z; z += GridSpacing)
                {
                    var pos = new Vector3(x + origin.X, y + origin.Y, z + origin.Z);
                    DebugDraw.Sphere(pos, .5f, Colors.WebGray, 5.0f);
                    _points.Add(pos);
                }
            } 
        }

        EmitSignal(SignalName.PointsCreated);
    }
}
