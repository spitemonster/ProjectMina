using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class AIMovementComponent : Node
{
	[Signal]
	public delegate void TargetReachedEventHandler();

	[Export]
	protected NavigationAgent3D _navigationAgent;

	private AICharacter _owner;
	private Godot.Collections.Array<Rid> _exclude = new();
	private Vector3 _lookPosition;

	public void SetTargetPosition(Vector3 newPosition, bool force = false)
	{
		// NavigationServer3D.MapGet
		Vector3 pos = NavigationServer3D.MapGetClosestPoint(_navigationAgent.GetNavigationMap(), newPosition);
		_navigationAgent.TargetPosition = newPosition;
	}

	public void SetLookPosition(Vector3 newLookPosition)
	{
		_lookPosition = newLookPosition;
	}

	public override void _Ready()
	{
		_owner = GetOwner<AICharacter>();
		_exclude.Add(_owner.GetRid());

		CallDeferred("InitMovement");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_navigationAgent == null)
		{
			return;
		}

		_owner.GlobalTransform = _owner.GlobalTransform.InterpolateWith(_owner.GlobalTransform.LookingAt(_lookPosition, Vector3.Up), 8.0f * (float)delta);

		Vector3 currentRotation = _owner.GlobalRotation;
		currentRotation.X = 0;
		currentRotation.Z = 0;
		_owner.GlobalRotation = currentRotation;

		if (_navigationAgent.IsNavigationFinished())
		{

		}
		else if (_navigationAgent.GetNextPathPosition() is Vector3 nextPathPosition)
		{
			float v = 80.0f * (_navigationAgent.DistanceToTarget() / 1.0f);
			_navigationAgent.Velocity = (nextPathPosition - _owner.GlobalPosition).Normalized() * v * (float)delta;
		}
	}

	private void SetMovementVelocity(Vector3 safeVelocity)
	{
		_owner.Velocity = safeVelocity;
		_owner.MoveAndSlide();
	}

	private void InitMovement()
	{
		if (_navigationAgent != null)
		{
			_navigationAgent.TargetPosition = _owner.GlobalPosition;
			_navigationAgent.VelocityComputed += SetMovementVelocity;
			_navigationAgent.TargetReached += () =>
			{
				EmitSignal(SignalName.TargetReached);
			};
		}
	}
}
