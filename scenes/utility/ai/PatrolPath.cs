using Godot;
namespace ProjectMina;

[GlobalClass]
public partial class PatrolPath : Node3D
{
	[Export]
	protected Godot.Collections.Array<Marker3D> patrolPoints;

	public Vector3 GetPatrolPointPosition(int patrolPointIndex)
	{
		return patrolPoints[patrolPointIndex].GlobalPosition;
	}

	public int GetPatrolPointsCount()
	{
		return patrolPoints.Count;
	}
}
