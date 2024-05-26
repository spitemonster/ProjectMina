using Godot;
using Godot.Collections;
using ProjectMina.BehaviorTree;


namespace ProjectMina;

public enum AIState : uint
{
	Idle,
	Patrol,
	Suspicious,
	Combat
}

/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

public partial class AgentComponent : ControllerComponent
{
	public AICharacter Pawn { get; private set; }
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	[Export] public BehaviorTreeComponent BehaviorTree { get; protected set; }
	[Export] public AIPerceptionComponent Perception { get; protected set; }

	public AIState AwarenessLevel = AIState.Idle;

	private BlackboardAsset _blackboardAsset;

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

		if (Blackboard.Blackboard is BlackboardAsset bb)
		{
			_blackboardAsset = bb;
		}
		
		GD.Print("[color=pink]should set max health[/color]");
		GD.Print("[color=pink]", Pawn.CharacterHealth.MaxHealth, "[/color]");

		Blackboard.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		Pawn.NavigationAgent.TargetReached += () =>
		{
			Blackboard?.SetValue("target_movement_position_reached", true);
			Blackboard?.SetValue("target_movement_position", Vector3.Zero);
		};

		Pawn.CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Blackboard?.SetValue("current_health", newHealth);

			if (wasDamage)
			{
				Blackboard?.SetValue("last_received_damage", Time.GetUnixTimeFromSystem());
			}
		};

		Perception.CharacterEnteredLineOfSight += (CharacterBase character) =>
		{

		};
		
		// if (CharacterPerception != null)
		// {
		// 	CharacterPerception.CharacterEnteredLineOfSight += (character) =>
		// 	{
		// 		if (_lookTarget == null)
		// 		{
		// 			_StartNoticeTimer(character);
		// 		}
		// 	};
		//
		// 	CharacterPerception.CharacterExitedLineOfSight += (character) =>
		// 	{
		// 		if (_targetCharacter == character)
		// 		{
		// 			_ClearNoticeTimer();
		// 		}
		// 		
		// 		if (CharacterPerception.GetNearestVisibleCharacter() is { } c)
		// 		{
		// 			_StartNoticeTimer(c);
		// 		}
		// 	};
		// }
		GD.Print("should start behavior tree");
		if (BehaviorTree == null)
		{
			GD.Print(" no behavior tree");
		}
		BehaviorTree?.Start(this);
	}

	public void SetTargetPosition(Vector3 position)
	{
		GD.Print("setting pawn target position");
		Pawn.SetTargetPosition(position);
	}
}
