using Godot;
namespace ProjectMina;

public partial class PatrolPathComponent : ComponentBase
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
