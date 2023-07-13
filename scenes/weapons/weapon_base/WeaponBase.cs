using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class WeaponBase : Node3D
{
	public enum WeaponType
	{
		Melee,
		Ranged
	};

	[Export]
	protected WeaponType weaponType = WeaponType.Melee;;

	[Export]
	protected Interaction InteractionComponent { get; private set; }

	[Export]
	protected Equipment EquipmentComponent { get; private set; }

	[Export]
	public double Damage { get; protected set; } = 10.0f;

	[Export]
	protected Animation AttackAnimation { get; set; }

	protected CharacterBase _wieldingCharacter;

	public virtual void Attack()
	{

	}

	public virtual void FinishAttack()
	{

	}

	public virtual void Equip(CharacterBase equippingCharacter)
	{
		_wieldingCharacter = equippingCharacter;
		_wieldingCharacter.Attacked += Attack;
		_wieldingCharacter.FinishedAttack += FinishAttack;
	}

	public virtual Animation GetAttackAnimation()
	{
		return AttackAnimation;
	}

	public override void _Ready()
	{
		EquipmentComponent.Equipped += Equip;
	}
}
