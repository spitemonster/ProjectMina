using Godot;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class Item : GodotObject
{
    public Vector3 Position { get; private set; }

    // tracks a point's score in the current context
    public float Score;
    
    // tracks the number of tests run on the point
    public int Tests;

    public Item(Vector3 position)
    {
        Position = position;
    }

    public void AddScore(float score)
    {
        Score += score;
        Tests++;
    }
}
