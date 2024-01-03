using Godot;

namespace ProjectMina;

public partial class MeleeWeapon : WeaponBase
{
	[Export]
	protected HitboxComponent _hitbox;

	public override void Attack()
	{
		base.Attack();
		if (_hitbox != null)
		{
			_hitbox.CanHit = true;
		}

		System.Diagnostics.Debug.Assert(_hitbox != null, "no hitbox");

	}

	public override void FinishAttack()
	{
		base.FinishAttack();
		if (_hitbox != null)
		{
			_hitbox.CanHit = false;
		}
	}

	public override void Equip(CharacterBase equippingCharacter)
	{
		base.Equip(equippingCharacter);

		if (_hitbox != null)
		{
			_hitbox.SetOwner(equippingCharacter);
		}
	}

	public override void _Ready()
	{
		base._Ready();

		weaponType = WeaponType.Melee;
	}
}