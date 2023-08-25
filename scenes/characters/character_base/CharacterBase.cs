using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{

	[Signal] public delegate void AttackedEventHandler();
	[Signal] public delegate void FinishedAttackEventHandler();
	[Signal] public delegate void FocusChangedEventHandler(Node3D newFocus);
	[Signal] public delegate void FocusGainedEventHandler();
	[Signal] public delegate void FocusLostEventHandler();

	[Export]
	public CollisionShape3D CharacterBody { get; protected set; }
	[Export]
	public HealthComponent CharacterHealthComponent { get; protected set; }
	public Vector3 ForwardVector { get; private set; }

	protected Node3D currentFocus;

	public override void _Ready()
	{
		CharacterHealthComponent.HealthDepleted += Die;
	}

	public override void _PhysicsProcess(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;
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

	public virtual void SetFocus(Node3D newFocus)
	{
		if (currentFocus == newFocus)
		{
			return;
		}

		if (currentFocus == null && newFocus != null)
		{
			EmitSignal(SignalName.FocusGained);
		}

		currentFocus = newFocus;
		EmitSignal(SignalName.FocusChanged, currentFocus);
	}

	public virtual void LoseFocus()
	{
		if (currentFocus == null)
		{
			return;
		}

		SetFocus(null);
		EmitSignal(SignalName.FocusLost);
	}

	public virtual void Die()
	{
		QueueFree();
	}
}