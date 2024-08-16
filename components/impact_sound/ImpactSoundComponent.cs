using Godot;
using System.Diagnostics;

namespace ProjectMina;

public partial class ImpactSoundComponent : ComponentBase
{
	[Export] protected RigidBody3D Body;
	[Export] protected float ImpactTimeoutDuration = .1f;
	[Export] protected AudioStreamWav ImpactSound;

	private bool _canImpact = true;

	private SoundPlayer3D _player;
	
	public override void _Ready()
	{
		base._Ready();
		
		if (Engine.IsEditorHint() || TimerPool.Manager == null)
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}
		
		Debug.Assert(Body != null && GetParent<RigidBody3D>() != null, "Impact sound component requires either its Body field populated or its parent to be of type RigidBody3D.");
		
		// attempt to automatically set Body if it's not set by the exported property
		if (Body == null && GetParent<RigidBody3D>() != null)
		{
			Body = GetParent<RigidBody3D>();
		}

		// _player = AudioPlayerPool.Manager.GetPlayer3D();
		
		if (EnableDebug) Debug.Assert(_player != null, "Impact Sound Component should have an audio player");
		if (EnableDebug) Debug.Assert(ImpactSound != null, "Impact Sound Component should have an impact sound");

		if (_player == null || ImpactSound == null)
		{
			return;
		}

		AddChild(_player);
		Body.BodyEntered += HandleImpact;	
	}

	private void HandleImpact(Node body)
	{
		if (body is RigidBody3D or StaticBody3D or CharacterBody3D && _canImpact)
		{
			PlayImpactSound();
			DisableImpact();
			var t = TimerPool.Manager.GetTimer(ImpactTimeoutDuration, EnableImpact, false, true, GetOwner<Node>().Name);
			t.Start();

			Node3D node = (Node3D)body;

			if (_player != null && AudioStreamPool.Manager != null)
			{
				_player.Stream = AudioStreamPool.Manager.GetRandomStreamFromCategory("impact_soft_wood");
				_player.GlobalPosition = node.GlobalPosition;
				_player.Play();
			}
		}
	}

	private void PlayImpactSound()
	{
	}

	private void DisableImpact()
	{
		_canImpact = false;
	}

	private void EnableImpact()
	{
		_canImpact = true;
	}
}
