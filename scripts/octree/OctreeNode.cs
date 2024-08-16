using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class OctreeNode
{

    
    public Vector3 Position => _bounds.Position;
    public Vector3 Size => _bounds.Size;
    public Vector3 Center => _bounds.GetCenter();
    private Aabb _bounds;
    // the smallest size this node (cube) can be per side
    private float _minSize;
    
    private Aabb[] _childBounds;
    
    private OctreeNode[] _children;
    
    private List<Vector3> _points;
    
    RandomNumberGenerator rng = new();
    
    public OctreeNode(Aabb bounds, float minSize)
    {
        _bounds = bounds;
    
        _minSize = minSize;
    
        var size = _bounds.Size.Y;
        var half = size / 2;
        
        if (half >= minSize)
        {
    
            _childBounds = new Aabb[8];
            _childBounds[0] = new Aabb(Position, new Vector3(size / 2,size / 2,size / 2));
            _childBounds[1] = new Aabb(Position + new Vector3(half, 0, 0), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[2] = new Aabb(Position + new Vector3(half, half, 0), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[3] = new Aabb(Position + new Vector3(half, 0, half), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[4] = new Aabb(Position + new Vector3(half, half, half), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[5] = new Aabb(Position + new Vector3(0, half, half), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[6] = new Aabb(Position + new Vector3(0, 0, half), new Vector3(size / 2,size / 2,size / 2));
            _childBounds[7] = new Aabb(Position + new Vector3(0, half, 0), new Vector3(size / 2,size / 2,size / 2));
        }
    
        _points = new();
    
        Draw(10f);
    }
    
    public List<Vector3> FindPositionsInRadius(Vector3 position, float radius)
    {
        var positions = new List<Vector3>();
        if (!_bounds.HasPoint(position))
        {
            return positions;
        }
        
        Draw(0f);
    
        if (_children != null)
        {
            foreach (var n in _children)
            {
                positions.AddRange(n.FindPositionsInRadius(position, radius));
            }
        }
        
        positions = positions.FindAll(point => position.DistanceTo(point) <= radius);
    
        return positions;
    }
    
    public void Add(Vector3 position) {
        DivideAndAdd(position);
    }
    
    public void DivideAndAdd(Vector3 position)
    {
        if (_childBounds is not { Length: 8 })
        {
            return;
        }
        
        
        if (!_bounds.HasPoint(position))
        {
            return;
        }
        
        if (_children == null || _children.Length < 8)
        {
            _children = new OctreeNode[8];
            
            for (int i = 0; i < 8; i++)
            {
                _children[i] = new OctreeNode(_childBounds[i], _minSize);
            }
        }
    
        for (int i = 0; i < 8; i++)
        {
            _children[i] = new OctreeNode(_childBounds[i], _minSize);
    
            if (_childBounds[i].HasPoint(position))
            {
                _children[i].DivideAndAdd(position);
                return;
            }
        }
        
        _points.Add(position);
    }
    
    public void Draw( float duration = 120f)
    {
        var randColor = new Color(rng.Randf(), rng.Randf(), rng.Randf());
        DebugDraw.Box(Center, _bounds.Size, Colors.Aqua, duration);
    }
}