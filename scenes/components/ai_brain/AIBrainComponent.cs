using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class AIBrainComponent : Node
{
	public Vector2 MovementDirection { get; protected set; }
	public Node3D CurrentFocus { get => _currentFocus; }

	[Export]
	protected AISightComponent _sightComponent;

	[Export]
	protected AIHearingComponent _hearingComponent;

	[Export]
	protected NavigationAgent3D _navigationAgent;

	[Export]
	public float TickRate
	{
		get { return _tickRate; }
		set { _tickRate = Mathf.Clamp(value, 0.07f, 60.0f); }
	}

	private float _tickRate = 0.1f;
	private Timer _tickTimer;
	private Node3D _currentFocus;
	private CombatGridPoint currentGridPoint;
	private AICharacter _owner;

	public override void _Ready()
	{
		_tickTimer = new()
		{
			WaitTime = _tickRate,
			Autostart = true,
			OneShot = false
		};

		_owner = GetOwner<AICharacter>();

		if (_sightComponent != null)
		{
			_sightComponent.CharacterEnteredLineOfSight += (character) =>
			{
				if (character is PlayerCharacter p && _currentFocus == null)
				{
					_currentFocus = p;
					currentGridPoint = p.CombatGrid.GetPoint();
					currentGridPoint.OccupyPoint(GetOwner<CharacterBase>());
				}
			};
		}

		_navigationAgent.TargetReached += () =>
		{
			GD.Print("should attack");
			_owner.Attack();
		};

		Debug.Assert(_sightComponent != null, "no ai sight component");

		_tickTimer.Timeout += Tick;

		AddChild(_tickTimer);
	}

	private void Tick()
	{
		if (_sightComponent == null || _hearingComponent == null || _navigationAgent == null)
		{
			Debug.Assert(false, "ai brain missing critical components");
			return;
		}

		if (_currentFocus == null)
		{
			return;
		}

		if (_currentFocus is PlayerCharacter p)
		{
			_navigationAgent.TargetPosition = p.GlobalPosition;
			// calculate movement direction and convert to vector 2
			Vector3 targetDirection = (_owner.GlobalPosition - _navigationAgent.GetNextPathPosition()).Normalized();
			float VecX = targetDirection.X;
			float VecY = targetDirection.Z;

			MovementDirection = new(VecX, VecY);
		}
	}
}
