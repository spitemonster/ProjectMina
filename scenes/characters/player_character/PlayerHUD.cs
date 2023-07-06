using System;
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
	protected ColorRect _reticle;

	private PlayerCharacter _player;

	public override void _Ready()
	{
		base._Ready();

		if (_interactionComponent != null)
		{
			_interactionComponent.InteractionStateChanged += UpdateReticleInteractionState;
		}

		_player = GetOwner<PlayerCharacter>();

		if (_player.CharacterHealthComponent != null)
		{
			// _player.CharacterHealthComponent.HealthChanged += () => { GD.Print("health changed from hud"); }
		}
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
