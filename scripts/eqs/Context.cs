using Godot;
using System;

public partial class QueryContext : Node
{
	public Vector3 Position { get; private set; }
	public Vector3 Extent { get; private set; }

	public QueryContext(Vector3 position, Vector3 extent)
	{
		// extent of the query should have a default value
		if (extent == Vector3.Zero)
		{
			Extent = new Vector3(10, 10, 10);
		}
		Position = position;
		Extent = extent;
	}
	
	public override void _Process(double delta)
	{
	}
}
