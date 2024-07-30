using Godot;
using System;

namespace ProjectMina;

public partial class Key : RigidBody3D
{
	[Export] public EKeyChannel KeyChannel { get; protected set; }
	
	private UsableComponent _usableComponent;
	public override void _Ready()
	{
		_usableComponent = GetNodeOrNull<UsableComponent>("Usable");

		if (_usableComponent != null)
		{
			_usableComponent.InteractionStarted += _Use;
		}
		else
		{
			return;
		}
	}

	private void _Use(CharacterBase character)
	{
		character.CharacterInteraction.Keychain.AddKey(KeyChannel);
		// TODO : determine how we deal with objects that should get destroyed on interact
		SetVisible(false);
	}
}
