
using System.Linq;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;
namespace ProjectMina.EnvironmentQuerySystem;

public enum EQueryTestType : int
{
    Filter,
    Score,
    Both
}

[Tool]
[GlobalClass]
public partial class Test : Node
{
    public EQueryTestType TestType = EQueryTestType.Both;

    // exclusive min -- must be ABOVE 0.0f
    public float MinScore = 0.0f;
    
    // inclusive max -- up to and INCLUDING 1.0f
    public float MaxScore = 1.0f;

    [Signal]
    public delegate void TestCompletedEventHandler(Array<Vector3> Points);
    
    // this is the function to override to calculate a point's score
    protected virtual float CalculateScore(Vector3 point)
    {
        return 0.0f;
    }
    
    // receives an array of items and returns the same sorted, filtered or both
    public virtual async Task<Array<Vector3>> RunTest(AgentComponent agent, Array<Vector3> items)
    {
        return await Task.Run(() =>
        {
            Godot.Collections.Dictionary<Vector3, float> scoreDict = new();

            foreach (var item in items)
            {
                var score = CalculateScore(item);

                // immediately filter out anything below the threshold if we should be filtering
                if (TestType is EQueryTestType.Filter or EQueryTestType.Both)
                {
                    if (score > MinScore && score <= MaxScore)
                    {
                        scoreDict.Add(item, score);
                    }
                }
                else
                {
                    scoreDict.Add(item, score);
                }
            }

            // return an empty array if all items were removed
            if (scoreDict.Count < 1)
            {
                return new Array<Vector3>();
            }

            // if we're supposed to sort the array, do that
            if (TestType is EQueryTestType.Score or EQueryTestType.Both)
            {
                // absolutely roundabout way to sort a dictionary and then convert it back to a dictionary
                // orderbydescending returns a generic c# enumerable so we need to cast it to a system dict from which we can make a new Godot dict
                System.Collections.Generic.Dictionary<Vector3, float> test =
                    new(scoreDict.OrderByDescending(pair => pair.Value));

                scoreDict = new Godot.Collections.Dictionary<Vector3, float>(test);
            }

            return (Array<Vector3>)scoreDict.Keys;
        });
    }
}