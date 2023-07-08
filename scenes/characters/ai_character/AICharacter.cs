using Godot;
namespace ProjectMina;

public enum AIState
{
	Idle,
	Suspicious,
	Alerted
}

public enum AIBehavior
{
	Patrol,
	Search,
	Attack,
	Wait
}

[GlobalClass]
public partial class AICharacter : CharacterBase
{

	[Export]
	protected NavigationAgent3D _navigationAgent;

	[Export]
	protected PatrolPath _patrolPath;

	private int _currentPatrolPointIndex = 0;

	public override void _Ready()
	{
		CallDeferred("InitMovement");
	}

	private void InitMovement()
	{
		if (_navigationAgent != null && _patrolPath != null)
		{
			_navigationAgent.TargetPosition = _patrolPath.GetPatrolPointPosition(0);
			_navigationAgent.VelocityComputed += SetMovementVelocity;
		}
	}

	private void SetMovementVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		LookAt(_navigationAgent.GetNextPathPosition());
		Vector3 currentRotation = GlobalRotation;
		currentRotation.X = 0;
		currentRotation.Z = 0;
		GlobalRotation = currentRotation;
		MoveAndSlide();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_navigationAgent != null)
		{
			if (_navigationAgent.IsNavigationFinished())
			{
				if (_currentPatrolPointIndex == _patrolPath.GetPatrolPointsCount() - 1)
				{
					_currentPatrolPointIndex = 0;

				}
				else
				{
					_currentPatrolPointIndex += 1;
				}

				_navigationAgent.TargetPosition = _patrolPath.GetPatrolPointPosition(_currentPatrolPointIndex);
			}


			Vector3 nextNavPosition = _navigationAgent.GetNextPathPosition();

			_navigationAgent.Velocity = (nextNavPosition - GlobalPosition).Normalized() * 2.0f;
		}


	}
}
