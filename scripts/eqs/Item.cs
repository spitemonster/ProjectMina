using Godot;
using System;

namespace ProjectMina.EQS;

[Tool]
[GlobalClass]
public partial class Point : GodotObject
{
    public Vector3 Position { get; private set; }

    // tracks a point's score in the current context
    public float Score;
    
    // tracks the number of tests run on the point
    public int Tests;

    public Point(Vector3 position)
    {
        Position = position;
    }
    
    public void 
}
