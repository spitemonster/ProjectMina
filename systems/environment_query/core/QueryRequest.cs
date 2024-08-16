using Godot;
using Godot.Collections;
// using ProjectMina;

namespace ProjectMina.EQS;
public partial class QueryRequest: RefCounted
{
    public AIControllerComponent Querier { get; private set; }
    public int ID { get; private set; }
    public EnvironmentQuery EnvironmentQuery { get; private set; }

    public QueryRequest(int id, AIControllerComponent querier, EnvironmentQuery environmentQuery)
    {
        ID = id;
        Querier = querier;
        EnvironmentQuery = environmentQuery;
    }

    public void Fulfill()
    {
        Free();
    }
}