using Godot;
using Godot.Collections;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{
	// signals
	[Signal] public delegate void AttackedEventHandler();
	[Signal] public delegate void FinishedAttackEventHandler();

	// publicly accessible components
	[Export] public CollisionShape3D CharacterBody { get; set; }
	[Export] public HealthComponent CharacterHealth { get; protected set; }
	[Export] public AttentionComponent CharacterAttention { get; protected set; }
	[Export] public MovementComponent CharacterMovement { get; protected set; }
	[Export] public InteractionComponent CharacterInteraction { get; protected set; }
	[Export] public SoundComponent CharacterSound { get; protected set; }
	[Export] public Marker3D Eyes { get; protected set; }
	[Export] public Marker3D Chest { get; protected set; }
	[Export] public AnimationPlayer AnimPlayer { get; protected set; }

	[Export] protected double RotationRate = 6.0;

	[Export] private bool _debug = false;

	public Vector3 ForwardVector { get; protected set; }

	protected double _footstepTimeout = .1;
	protected bool _canFootstep = true;
	private Timer _footstepTimer;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}

		_footstepTimer = new()
		{
			WaitTime = .5,
			Autostart = false,
			OneShot = true
		};

		_footstepTimer.Timeout += () =>
		{
			GD.Print("footstep timeout");
			_canFootstep = true;
		};

		AddChild(_footstepTimer);

		CharacterHealth.HealthDepleted += Die;
	}

	public override void _PhysicsProcess(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;

		if (_debug)
		{
			Vector3 traceOrigin = Chest.GlobalPosition;
			Vector3 traceEnd = Chest.GlobalPosition + ForwardVector * 2f;
			DebugDraw.Line(Chest.GlobalPosition, traceEnd, Colors.Red);
		}
	}

	// intended to be called by the logic of the entity controlling the character; the ai brain or the player character
	public virtual void Attack()
	{
		EmitSignal(SignalName.Attacked);
	}

	// typically triggered by animations signalling that the character is able to attack again
	public virtual bool FinishAttack()
	{
		EmitSignal(SignalName.FinishedAttack);
		return false;
	}

	public virtual void Die()
	{
		if (_debug)
		{
			DebugDraw.Text(Name + " is dead.");
		}

		QueueFree();
	}

	public virtual void Footstep()
	{
		GD.Print("footstep!");
	}

	public override string[] _GetConfigurationWarnings()
	{
		base._GetConfigurationWarnings();
		Array<string> warnings = new();

		if (Eyes == null)
		{
			warnings.Add("Character Eyes property must not be empty.");
		}

		if (Chest == null)
		{
			warnings.Add("Character Chest property must not be empty.");
		}

		string[] baseWarnings = base._GetConfigurationWarnings();
		if (baseWarnings != null && baseWarnings.Length > 0)
		{
			warnings.AddRange(baseWarnings);
		}

		string[] errs = new string[warnings.Count];

		for (int i = 0; i < warnings.Count; i++)
		{
			errs.SetValue(warnings[i], i);
		}

		return errs;
	}
}