using Godot;
using System;

namespace ProjectMina;

public partial class DamageComponent : ComponentBase
{
	[Export] private DamageStats _damageStats;

	public float GetDamageAmount()
	{
		return _damageStats.DamageAmount;
	}
}
