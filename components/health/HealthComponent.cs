using Godot;

namespace ProjectMina;

public partial class HealthComponent : ComponentBase
{
	[Export] public double MaxHealth { get; protected set; } = 100.0;
	[Export] public double CurrentHealth { get; protected set; }
	[Signal] public delegate void HealthChangedEventHandler(double newHealth, bool wasDamage);
	[Signal] public delegate void HealthMaxedEventHandler();
	[Signal] public delegate void HealthDepletedEventHandler();
	
	public override void _Ready()
	{
		base._Ready();
		CurrentHealth = MaxHealth;
	}

	public void FullyHeal()
	{
		CurrentHealth = MaxHealth;
		
		EmitSignal(SignalName.HealthMaxed);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, false);
	}

	public void FullyHurt(bool isDamage = true)
	{
		CurrentHealth = 0;
		EmitSignal(SignalName.HealthDepleted);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, isDamage);
	}

	public void TakeDamage(float amount)
	{
		ChangeHealth(amount, true);
		GD.Print(GetOwner().Name, " took damage. Current health: ", CurrentHealth);
	}

	// force overwrites the health entirely
	public void ChangeHealth(double amount, bool isDamage = false, double force = -1.0)
	{
		amount = Mathf.Abs(amount);
		CurrentHealth = (force > 0.0) ? force : CurrentHealth + (isDamage ? -amount : amount);
		CurrentHealth = Mathf.Max(0.0, Mathf.Min(CurrentHealth, MaxHealth));

		if (CurrentHealth >= MaxHealth)
		{
			EmitSignal(SignalName.HealthMaxed);
		}
		else if (CurrentHealth == 0.0)
		{
			EmitSignal(SignalName.HealthDepleted);
		}

		EmitSignal(SignalName.HealthChanged, CurrentHealth, isDamage);
	}
}
