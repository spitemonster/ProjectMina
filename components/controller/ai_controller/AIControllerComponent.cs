using Godot;
using ProjectMina.BehaviorTree;


namespace ProjectMina;

/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

[GlobalClass,Icon("res://resources/images/icons/icon--brain.svg")]
public partial class AIControllerComponent : ControllerComponent
{
	[Signal] public delegate void AgentStateChangedEventHandler(EAgentState agentState);
	// public AICharacter Pawn { get; private set; }
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	[Export] public BehaviorTreeComponent BehaviorTree { get; protected set; }
	[Export] public AIPerceptionComponent Perception { get; protected set; }
	[Export] public NavigationAgent3D NavigationAgent { get; protected set; }

	public EAgentState AgentState = EAgentState.Idle;
	private BlackboardAsset _blackboardAsset;
	// private PlayerCharacter _pc;
	private AICharacter _aiPawn;
	
	public Node3D GetCurrentFocus()
	{
		return (Node3D)Blackboard.GetValueAsObject("focus");
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)Blackboard.GetValue("target_position");
	}

	public override void _Ready()
	{
		// none of this needs to run in the editor and it DEFINITELY does not need to tick in the editor
		if (Engine.IsEditorHint())
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}
		
		Pawn = GetOwner<AICharacter>();
		_aiPawn = GetOwner<AICharacter>();
		
		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(Blackboard != null, "AI Brain Component does not have Blackboard");
			System.Diagnostics.Debug.Assert(BehaviorTree != null, "AI Brain Component does not have Blackboard");
			System.Diagnostics.Debug.Assert(Pawn != null, "AI Brain Component not attached to AI Character.");
		}
		
		if (!Active || Blackboard == null || BehaviorTree == null || Pawn == null)
		{
			return;
		}
		
		base._Ready();

		if (Blackboard.Blackboard is { } bb)
		{
			_blackboardAsset = bb;
		}

		Blackboard.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		if (NavigationAgent == null)
		{
			GD.PushError(" no navigation agent in ready either");
		}
		else
		{
			NavigationAgent.TargetReached += () =>
			{
				Blackboard?.SetValue("target_position", Vector3.Zero);
			};
		}

		if (_aiPawn.CharacterPerception != null)
		{
			_aiPawn.CharacterPerception.CharacterNoticed += _CharacterNoticed;
			_aiPawn.CharacterPerception.CharacterSeen += _CharacterSeen;
			_aiPawn.CharacterPerception.CharacterLostVisualDetection += _LostVisual;
		}

		Pawn.CharacterHealth.HealthChanged += (newHealth, wasDamage) =>
		{
			Blackboard?.SetValue("health", newHealth);

			if (wasDamage)
			{
				Blackboard?.SetValue("last_received_damage", Time.GetUnixTimeFromSystem());
			}
		};

		// Perception.PointOfInterestNoticed += _NoticePointOfInterest;
		// Perception.PointOfInterestSeen += _SeePointOfInterest;
		CallDeferred("_InitDev");
		CallDeferred("_SetupBehaviorTree");
	}

	private void _SetState(EAgentState newState)
	{
		if (AgentState == newState)
		{
			return;
		}

		AgentState = newState;
		EmitSignal(SignalName.AgentStateChanged, (uint)AgentState);
	}

	private void _LostVisual(CharacterBase character)
	{
		_SetState(EAgentState.Idle);
		Blackboard.SetValueAsInt("state", (int)AgentState);
		Blackboard.SetValueAsObject("focus", null);
		_aiPawn.AwarenessLabel.Text = "Idle";
		_aiPawn.SheathWeapon();
	}

	private void _InitDev()
	{
		_aiPawn.AwarenessLabel.Text = "Idle";
	}

	private void _CharacterNoticed(CharacterBase character)
	{
		if (AgentState == EAgentState.Combat)
		{
			return;
		}
		
		_SetState(EAgentState.Suspicious);
		Blackboard.SetValueAsInt("state", (int)AgentState);
		Blackboard.SetValueAsObject("focus", character);
		_aiPawn.AwarenessLabel.Text = "Suspicious";
	}

	private void _CharacterSeen(CharacterBase character)
	{
		if (AgentState == EAgentState.Combat)
		{
			return;
		}
		
		_SetState(EAgentState.Combat);
		Blackboard.SetValueAsInt("state", (int)AgentState);
		Blackboard.SetValueAsObject("focus", character);
		_aiPawn.DrawWeapon();
		_aiPawn.AwarenessLabel.Text = "Alerted";
	}

	private void _SetupBehaviorTree()
	{
		BehaviorTree?.SetController(this);
		BehaviorTree?.Start();
	}

	private void _NoticePointOfInterest(Node3D pointOfInterest)
	{
		Blackboard.SetValueAsObject("focus", pointOfInterest);
		// Blackboard.SetValueAsString("state", "suspicious");
	}
	
	private void _SeePointOfInterest(Node3D pointOfInterest)
	{
		Blackboard.SetValueAsObject("focus", pointOfInterest);
		// Blackboard.SetValueAsString("state", "alerted");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (NavigationAgent == null || Blackboard == null)
		{
			return;
		}
		
		Node3D target = (Node3D)Blackboard?.GetValueAsObject("focus");
		var movementDirection = new Vector2();
		
		// movement
		if (NavigationAgent.TargetPosition != Vector3.Zero && !NavigationAgent.IsTargetReached())
		{
			var targetDirection = (NavigationAgent.GetNextPathPosition() - Pawn.GlobalPosition).Normalized();
			movementDirection = new Vector2(targetDirection.X, targetDirection.Z);
			
			Pawn.SetMovementInput(movementDirection);
		}
		else
		{
			Pawn.SetMovementInput(Vector2.Zero);
		}
		
		// rotation
		if (target != null)
		{
			var lookDirection = (target.GlobalPosition - Pawn.GlobalPosition).Normalized();
			Pawn.SetControlInput(new Vector2(lookDirection.X, lookDirection.Z));
		}
		else
		{
			Pawn.SetControlInput(movementDirection);
		}
	}

	public void SetFocus(Node3D focus)
	{
		Blackboard.SetValueAsObject("focus", focus);
	}

	public void SetTargetPosition(Vector3 position)
	{
		Blackboard.SetValueAsVector3("target_position", position);
		NavigationAgent.TargetPosition = position;
	}
}
