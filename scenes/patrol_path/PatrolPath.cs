using Godot;
namespace ProjectMina;

[GlobalClass]
public partial class PatrolPath : Node3D
{
	[Export] public Godot.Collections.Array<PatrolPoint> Points { get; protected set; }

	public Vector3 GetPatrolPointPosition(int patrolPointIndex)
	{
		return Points[patrolPointIndex].GlobalPosition;
	}

	public int GetPatrolPointsCount()
	{
		return Points.Count;
	}
}
