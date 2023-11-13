using System;
using Godot;
namespace ProjectMina;

[GlobalClass]
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
	private double _maxHealth;

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
			_maxHealth = _healthComponent.MaxHealth;

			_healthComponent.HealthChanged += (double newHealth, bool wasDamage) =>
			{
				Dev.UI.PushDevNotification((Mathf.Round((newHealth / _maxHealth) * 100) / 100).ToString());
				_healthBar.Value = Mathf.Round((newHealth / _maxHealth) * 100) / 100;
			};
		}

		Debug.Assert(_healthComponent != null, "no health component");
		Debug.Assert(_healthBar != null, "no health bar");
	}

	private void UpdateReticleInteractionState(InteractionComponent.InteractionType newState)
	{
		switch (newState)
		{
			case InteractionComponent.InteractionType.None:
				_reticle.Modulate = reticleDefaultColor;
				break;
			case InteractionComponent.InteractionType.Use:
				_reticle.Modulate = reticleUseColor;
				break;
			case InteractionComponent.InteractionType.Grab:
				_reticle.Modulate = reticleGrabColor;
				break;
			case InteractionComponent.InteractionType.Equip:
				_reticle.Modulate = reticleEquipColor;
				break;
		}
	}
}
