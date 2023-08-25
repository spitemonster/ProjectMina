using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

[GlobalClass]
public partial class AIBrainComponent : Node
{
	public Vector2 MovementDirection { get; protected set; }

	[Export] protected AISightComponent _sightComponent;
	[Export] protected AIHearingComponent _hearingComponent;
	[Export] protected NavigationAgent3D _navigationAgent;
	[Export] protected BehaviorTreeComponent _behaviorTree;
	[Export] protected BlackboardComponent _blackboard;
	[Export] protected AttentionComponent _attentionComponent;

	[Export]
	public float TickRate
	{
		get { return _tickRate; }
		set { _tickRate = Mathf.Clamp(value, 0.07f, 60.0f); }
	}

	private float _tickRate = 0.1f;
	private Timer _tickTimer;
	private CombatGridPoint currentGridPoint;
	private AICharacter _owner;

	public Node3D GetCurrentFocus()
	{
		NodePath currentFocusPath = (NodePath)_blackboard.GetValue("current_focus");

		if (currentFocusPath == new NodePath() || currentFocusPath == null)
		{
			return null;
		}

		if (GetNode(currentFocusPath) is Node3D node)
		{
			return node;
		}
		else
		{
			return null;
		}
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)_blackboard.GetValue("target_position");
	}

	public override void _Ready()
	{
		_owner = GetOwner<AICharacter>();
		_tickTimer = new()
		{
			WaitTime = _tickRate,
			Autostart = true,
			OneShot = false
		};
		_tickTimer.Timeout += Tick;
		AddChild(_tickTimer);

		System.Diagnostics.Debug.Assert(_attentionComponent != null, "no attention component");
		System.Diagnostics.Debug.Assert(_sightComponent != null, "no ai sight component");
		System.Diagnostics.Debug.Assert(_hearingComponent != null, "no ai hearing component");
		System.Diagnostics.Debug.Assert(_navigationAgent != null, "no navigation component");
		System.Diagnostics.Debug.Assert(_behaviorTree != null, "no behavior tree component");
		System.Diagnostics.Debug.Assert(_blackboard != null, "no blackboard component");
		System.Diagnostics.Debug.Assert(_owner != null, "owner is not ai character");
		System.Diagnostics.Debug.Assert(_tickTimer != null, "tick timer is null");

		_sightComponent.CharacterEnteredSightRadius += (character) =>
		{
			if (character is PlayerCharacter p)
			{
				// SeeEnemy(p);
			}
		};

		_sightComponent.CharacterExitedSightRadius += (character) =>
		{
			if (character.Equals(GetCurrentFocus()))
			{
				_owner.LoseFocus();
			}
		};

		_owner.MovementComponent.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			_blackboard.SetValue("movement_state", (int)newState);
		};

		_owner.NavigationAgent.TargetReached += () =>
		{
			_blackboard.SetValue("target_position_reached", true);
		};

		_attentionComponent.FocusChanged += (Node3D newFocus, Node3D previousFocus) =>
		{
			if (_blackboard.SetValue("current_focus", newFocus?.GetPath()))
			{
				GD.Print("successfully changed focus!");
			}

			if (newFocus == null)
			{
				GD.Print("lost focus!");
			}
		};

	}

	private void Tick()
	{
		if (_sightComponent == null || _hearingComponent == null || _navigationAgent == null)
		{
			Debug.Assert(false, "ai brain missing critical components");
			return;
		}
	}

	private void SeeEnemy(CharacterBase targetCharacter)
	{
		_owner.SetFocus(targetCharacter);
	}
}
