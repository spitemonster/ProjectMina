using Godot;

namespace ProjectMina;

public interface IQueryTest
{
    // returns the score of a given point
    public abstract float RunTest(QueryItem item, QueryContext context);
}