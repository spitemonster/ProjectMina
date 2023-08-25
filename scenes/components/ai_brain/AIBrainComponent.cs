using Godot;
namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class AIBrainComponent : Node
{
	public Vector2 MovementDirection { get; protected set; }
	public Node3D CurrentFocus { get => _currentFocus; }

	[Signal] public delegate void FocusedEventHandler(Node3D newFocus);
	[Signal] public delegate void LostFocusEventHandler();

	[Export] protected AISightComponent _sightComponent;

	[Export] protected AIHearingComponent _hearingComponent;

	[Export] protected NavigationAgent3D _navigationAgent;

	[Export] protected BehaviorTree.BehaviorTreeComponent _behaviorTree;

	[Export] protected BlackboardComponent _blackboard;

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

	public void Focus(Node3D newFocus)
	{
		if (newFocus == _currentFocus)
		{
			return;
		}

		_currentFocus = newFocus;
		EmitSignal(SignalName.Focused, _currentFocus);
	}

	public void LoseFocus()
	{
		_currentFocus = null;
		EmitSignal(SignalName.LostFocus);
	}

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
				GD.Print("character entered sight area");

				if (character is PlayerCharacter p && _currentFocus == null)
				{
					GD.Print("character is player character");

				}
			};
		}

		_owner.MovementComponent.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			_blackboard?.SetValue("movement_state", (int)newState);
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
	}
}
