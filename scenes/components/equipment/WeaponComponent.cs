using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class WeaponComponent : EquippableComponent
{
	[Signal] public delegate void AttackStartedEventHandler();
	[Signal] public delegate void AttackEndedEventHandler();

	[Export] public EWeaponRange Range = EWeaponRange.Melee;
	[Export] public float Damage = 10.0f;

	private bool _canAttack = true;

	public virtual void StartAttack()
	{
		_canAttack = false;
		EmitSignal(SignalName.AttackStarted);
	}

	public virtual void FinishAttack()
	{
		_canAttack = true;
		EmitSignal(SignalName.AttackEnded);
	}
}
