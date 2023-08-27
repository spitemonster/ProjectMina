using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{

	// signals
	[Signal] public delegate void AttackedEventHandler();
	[Signal] public delegate void FinishedAttackEventHandler();

	// publicly accessible components
	[Export] public CollisionShape3D CharacterBody { get; protected set; }
	[Export] public HealthComponent CharacterHealth { get; protected set; }
	[Export] public AttentionComponent CharacterAttention { get; protected set; }
	[Export] public MovementComponent CharacterMovement { get; protected set; }
	[Export] public InteractionComponent CharacterInteraction { get; protected set; }
	[Export] public SoundComponent CharacterSound { get; protected set; }

	// used by AI for looking at player character depending on their state
	[Export] public Marker3D Eyes { get; protected set; }
	[Export] public Marker3D Chest { get; protected set; }

	public MovementComponent testComponent;

	[Export] private bool _debug = false;

	public Vector3 ForwardVector { get; private set; }

	public override void _Ready()
	{
		CharacterHealth.HealthDepleted += Die;
	}

	public override void _PhysicsProcess(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;

		if (_debug)
		{
			Vector3 traceOrigin = Chest.GlobalPosition;
			Vector3 traceEnd = Chest.GlobalPosition + ForwardVector * 2f;
			DebugDraw.Line(Chest.GlobalPosition, traceEnd, Colors.Red);
		}
	}

	// intended to be called by the logic of the entity controlling the character; the ai brain or the player character
	public virtual void Attack()
	{
		EmitSignal(SignalName.Attacked);
	}

	// typically triggered by animations signalling that the character is able to attack again
	public virtual bool FinishAttack()
	{
		EmitSignal(SignalName.FinishedAttack);
		return false;
	}

	// shortcut methods for setting and getting focus
	public virtual void SetFocus(Node3D newFocus)
	{
		CharacterAttention.SetFocus(newFocus);
	}

	public virtual void LoseFocus()
	{
		CharacterAttention.LoseFocus();
	}

	public virtual Node3D GetFocus()
	{
		return CharacterAttention.CurrentFocus;
	}

	public virtual void Die()
	{
		if (_debug)
		{
			DebugDraw.Text(Name + " is dead.");
		}

		QueueFree();
	}

	public CharacterBase()
	{
		testComponent = new();
		AddChild(testComponent);
	}
}