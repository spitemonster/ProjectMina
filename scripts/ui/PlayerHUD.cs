using System;
using System.Diagnostics;
using Godot;
namespace ProjectMina;

public partial class PlayerHUD : Control
{
	[Export] protected Color ReticleDefaultColor;
	[Export] protected Color ReticleEnemyColor;
	[Export] protected Color ReticleAllyColor;
	[Export] private InteractionComponent _interactionComponent;
	private HealthComponent _healthComponent;
	
	[Export] private ColorRect _reticle;
	[Export] private ProgressBar _healthBar;
	[Export] protected VBoxContainer _indicatorContainer;
	[Export] private Control _useIndicator;
	[Export] private Control _grabIndicator;
	[Export] private Control _equipIndicator;

	private string _useText;

	private PlayerCharacter _player;

	public override void _Ready()
	{
		base._Ready();
		
		Global.Data.PlayerSet += _Setup;
		
	}

	private void _Setup(PlayerCharacter player)
	{
		_player = player;
		_interactionComponent = _player.CharacterInteraction;
		_healthComponent = _player.CharacterHealth;

		if (_useIndicator == null && _indicatorContainer.GetNodeOrNull("UseIndicator") is Control u)
		{
			_useIndicator = u;
		}
		
		if (_equipIndicator == null && _indicatorContainer.GetNodeOrNull("EquipIndicator") is Control e)
		{
			_equipIndicator = e;
		}
		
		if (_grabIndicator == null && _indicatorContainer.GetNodeOrNull("GrabIndicator") is Control g)
		{
			_grabIndicator = g;
		}
		
		_useIndicator.Visible = false;
		_grabIndicator.Visible = false;
		_equipIndicator.Visible = false;
		// _indicatorContainer?.QueueSort();

		if (_useIndicator is RichTextLabel l)
		{
			_useText = l.Text;
		}

		if (_interactionComponent != null)
		{
			_interactionComponent.InteractionContextUpdated += UpdateReticleInteractionState;
		}

		Debug.Assert(_interactionComponent != null, "no interaction component");

		if (_healthComponent != null && _healthBar != null)
		{
			_healthComponent.HealthChanged += UpdateHealthBar;
		}

		Debug.Assert(_healthComponent != null, "no health component");
		Debug.Assert(_healthBar != null, "no health bar");
	}

	private void UpdateReticleInteractionState(InteractionContext newContext)
	{
		_useIndicator?.SetVisible(newContext.Use);
		_grabIndicator?.SetVisible(newContext.Grab);
		_equipIndicator?.SetVisible(newContext.Equip);

		if (_useIndicator is RichTextLabel l)
		{
			if (newContext.Use && !String.IsNullOrEmpty(newContext.UsePromptOverride))
			{
				l.Text = newContext.UsePromptOverride;
			}
			else
			{
				l.Text = _useText;
			}
		}

		
	}

	private void UpdateHealthBar(double newHealth, bool wasDamage)
	{
		_healthBar.Value = Mathf.Round((newHealth / _healthComponent.MaxHealth) * 100) / 100;
	}
}
