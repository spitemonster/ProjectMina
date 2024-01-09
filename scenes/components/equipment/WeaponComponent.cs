using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class WeaponComponent : EquippableComponent
{
	public enum WeaponRange : uint
	{
		Melee,
		Ranged
	};

	[Export] public WeaponRange WeaponType = WeaponRange.Melee;
	[Export] public float Damage = 10.0f;
}
