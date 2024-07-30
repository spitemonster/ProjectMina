using Godot;
using ProjectMina.BehaviorTree;


namespace ProjectMina;

/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

[GlobalClass,Icon("res://_dev/icons/icon--brain.svg")]
public partial class AIControllerComponent : ControllerComponent
{
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
		return Pawn.CharacterAttention.CurrentFocus;
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)Blackboard.GetValue("target_movement_position");
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
		
		GD.Print("Owner: ", GetOwner().Name);
		
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
				Blackboard?.SetValue("target_movement_position_reached", true);
				Blackboard?.SetValue("target_movement_position", Vector3.Zero);
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
			Blackboard?.SetValue("current_health", newHealth);

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

	private void _LostVisual(CharacterBase character)
	{
		AgentState = EAgentState.Idle;
		Blackboard.SetValueAsInt("current_state", (int)AgentState);
		Blackboard.SetValueAsObject("current_target", null);
		_aiPawn.AwarenessLabel.Text = "Idle";
	}

	private void _InitDev()
	{
		_aiPawn.AwarenessLabel.Text = "Idle";
	}

	private void _CharacterNoticed(CharacterBase character)
	{
		AgentState = EAgentState.Suspicious;
		Blackboard.SetValueAsInt("current_state", (int)AgentState);
		Blackboard.SetValueAsObject("current_target", character);
		_aiPawn.AwarenessLabel.Text = "Suspicious";
	}

	private void _CharacterSeen(CharacterBase character)
	{
		AgentState = EAgentState.Combat;
		Blackboard.SetValueAsInt("current_state", (int)AgentState);
		Blackboard.SetValueAsObject("current_target", character);
		_aiPawn.AwarenessLabel.Text = "Alerted";
	}

	private void _SetupBehaviorTree()
	{
		BehaviorTree?.SetController(this);
		BehaviorTree?.Start();
	}

	private void _NoticePointOfInterest(Node3D pointOfInterest)
	{
		GD.Print("should notice point of interest");
		Blackboard.SetValueAsObject("current_target", pointOfInterest);
		// Blackboard.SetValueAsString("current_state", "suspicious");
	}
	
	private void _SeePointOfInterest(Node3D pointOfInterest)
	{
		GD.Print("should see point of interest");
		Blackboard.SetValueAsObject("current_target", pointOfInterest);
		// Blackboard.SetValueAsString("current_state", "alerted");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (NavigationAgent.TargetPosition != Vector3.Zero && !NavigationAgent.IsTargetReached())
		{
			var targetDirection = (NavigationAgent.GetNextPathPosition() - Pawn.GlobalPosition).Normalized();
			var movementDirection = new Vector2(targetDirection.X, targetDirection.Z);
			
			Pawn.SetMovementInput(movementDirection);
			Pawn.SetControlInput(movementDirection);
		}
		else
		{
			Pawn.SetMovementInput(Vector2.Zero);
			
			
		}
	}

	public void SetTargetPosition(Vector3 position)
	{
		Blackboard.SetValueAsVector3("target_movement_position", position);
		NavigationAgent.TargetPosition = position;
	}
}
