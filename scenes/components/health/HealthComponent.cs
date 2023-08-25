using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class HealthComponent : Node
{
	public double MaxHealth => _maxHealth;

	[Export]
	public double _maxHealth = 100.0;

	[Signal]
	public delegate void HealthChangedEventHandler(double newHealth, bool wasDamage);

	[Signal]
	public delegate void HealthMaxedEventHandler();

	[Signal]
	public delegate void HealthDepletedEventHandler();

	protected double _currentHealth;

	public override void _Ready()
	{
		base._Ready();
		_currentHealth = MaxHealth;
	}

	// force overwrites the health entirely
	public void ChangeHealth(double amount, bool isDamage = false, double force = -1.0)
	{

		amount = Mathf.Abs(amount);
		_currentHealth = (force > 0.0) ? force : _currentHealth + (isDamage ? -amount : amount);
		_currentHealth = Mathf.Max(0.0, Mathf.Min(_currentHealth, _maxHealth));

		if (_currentHealth == _maxHealth)
		{
			_ = EmitSignal(SignalName.HealthMaxed);
		}
		else if (_currentHealth == 0.0)
		{
			_ = EmitSignal(SignalName.HealthDepleted);
		}

		_ = EmitSignal(SignalName.HealthChanged, _currentHealth, isDamage);
	}

	public void UpdateHealth(double amount)
	{
		_currentHealth = amount;
	}
}
