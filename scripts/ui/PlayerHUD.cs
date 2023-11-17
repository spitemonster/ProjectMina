using System.Diagnostics;
using Godot;
namespace ProjectMina;

public partial class PlayerHUD : Control
{
	[Export]
	protected Color reticleDefaultColor;
	[Export]
	protected Color reticleUseColor;
	[Export]
	protected Color reticleGrabColor;
	[Export]
	protected Color reticleEquipColor;
	[Export]
	protected InteractionComponent _interactionComponent;
	[Export]
	protected HealthComponent _healthComponent;
	[Export]
	protected ColorRect _reticle;
	[Export]
	protected ProgressBar _healthBar;

	private PlayerCharacter _player;

	public override void _Ready()
	{
		base._Ready();

		_player = Global.Data.Player;

		_interactionComponent = _player.CharacterInteraction;
		_healthComponent = _player.CharacterHealth;

		if (_interactionComponent != null)
		{
			_interactionComponent.InteractionStateChanged += UpdateReticleInteractionState;
		}

		Debug.Assert(_interactionComponent != null, "no interaction component");

		if (_healthComponent != null && _healthBar != null)
		{
			_healthComponent.HealthChanged += UpdateHealthBar;
		}

		Debug.Assert(_healthComponent != null, "no health component");
		Debug.Assert(_healthBar != null, "no health bar");
	}

	private void UpdateReticleInteractionState(InteractionComponent.InteractionType newState)
	{
		_reticle.Modulate = newState switch
		{
			InteractionComponent.InteractionType.None => reticleDefaultColor,
			InteractionComponent.InteractionType.Use => reticleUseColor,
			InteractionComponent.InteractionType.Grab => reticleGrabColor,
			InteractionComponent.InteractionType.Equip => reticleEquipColor,
			_ => _reticle.Modulate
		};
	}

	private void UpdateHealthBar(double newHealth, bool wasDamage)
	{
		_healthBar.Value = Mathf.Round((newHealth / _healthComponent.MaxHealth) * 100) / 100;
	}
}
