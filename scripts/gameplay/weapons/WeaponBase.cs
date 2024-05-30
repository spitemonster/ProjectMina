using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class WeaponBase : RigidBody3D
{
	public Godot.Collections.Array<Node3D> Exclude { get; protected set; } = new();

	[Export] public EWeaponType weaponType { get; protected set; } = EWeaponType.Melee;

	[Export] public EquippableComponent Equipment { get; private set; }

	[Export] public double Damage { get; protected set; } = 10.0f;

	[Export] protected Animation AttackAnimation { get; set; }

	protected CharacterBase _wieldingCharacter;


	public bool AddExclude(Node3D node)
	{
		if (!Exclude.Contains(node))
		{
			Exclude.Add(node);
			return true;
		}
		else
		{
			return false;
		}
	}

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
		Exclude.Add(_wieldingCharacter);
	}

	public virtual Animation GetAttackAnimation()
	{
		return AttackAnimation;
	}

	public override void _Ready()
	{
		Equipment.Equipped += Equip;
		Equipment.Type = EquippableComponent.EquipmentType.Weapon;
		Exclude = new() {
			this,
			GetOwner<Node3D>()
		};
	}
}
