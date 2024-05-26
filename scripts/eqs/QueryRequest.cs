using Godot;
using Godot.Collections;
// using ProjectMina;

namespace ProjectMina.EnvironmentQuerySystem;
public partial class QueryRequest: GodotObject
{
    public AgentComponent Querier { get; private set; }
    public int ID { get; private set; }
    public Query Query { get; private set; }

    public QueryRequest(int id, AgentComponent querier, Query query)
    {
        ID = id;
        Querier = querier;
        Query = query;
    }

    public void Fulfill()
    {
        Free();
    }
}