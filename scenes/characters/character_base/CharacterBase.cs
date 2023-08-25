using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{

	[Signal]
	public delegate void AttackedEventHandler();
	[Signal]
	public delegate void FinishedAttackEventHandler();

	[Export]
	public CollisionShape3D CharacterBody { get; protected set; }
	[Export]
	public HealthComponent CharacterHealthComponent { get; protected set; }
	public Vector3 ForwardVector { get; private set; }

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

	public virtual void Die()
	{
		QueueFree();
	}
}