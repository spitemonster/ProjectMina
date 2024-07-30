using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class ParticleSystemComponent : ComponentBase3D
{
	[Signal] public delegate void FinishedEventHandler();
	[Export] public bool Loop { get; protected set; } = false;
	[Export] public bool Autostart { get; protected set; } = false;

	public bool FreeOnFinish = true;
	public bool Playing { get; private set; } = false;

	protected AnimationPlayer animationPlayer;
	
	private Godot.Collections.Array<Node> _particles = new();
	private int _completedParticleCount = 0;

	public void Play()
	{
		if (_particles.Count < 1)
		{
			return;
		}

		if (animationPlayer != null && animationPlayer.GetAnimationList().Length != 0)
		{
			// if there's an animation player and it has animations
			animationPlayer.Play();
		}
		else
		{
			foreach (var particle in _particles)
			{
				if (particle is CpuParticles3D c)
				{
					c.Emitting = true;
					c.Finished += AccumulateCompletedParticles;
				}
				else if (particle is GpuParticles3D g)
				{
					g.Emitting = true;
					g.Finished += AccumulateCompletedParticles;
				}
			}

			Playing = true;
		}
	}

	public override void _Ready()
	{
		animationPlayer ??= GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");

		foreach (Node child in GetChildren())
		{
			if (child is CpuParticles3D || child is GpuParticles3D)
			{
				if (child is CpuParticles3D c)
				{
					c.OneShot = !Loop;
					c.Emitting = false;
				}
				else if (child is GpuParticles3D g)
				{
					g.OneShot = !Loop;
					g.Emitting = false;
				}

				_particles.Add(child);
			}
		}

		if (Autostart)
		{
			Play();
		}
	}

	public override string[] _GetConfigurationWarnings()
	{
		Godot.Collections.Array<string> warnings = new();

		if (GetChildCount() == 0)
		{
			warnings.Add("Expected at least one child of type CPUParticles3D or GPUParticles3D.");
		}
		else
		{
			for (int i = 0; i < GetChildCount(); i++)
			{
				Node currentChild = GetChildren()[i];
				if (currentChild is not CpuParticles3D && currentChild is not GpuParticles3D && currentChild is not AnimationPlayer)
				{
					warnings.Add(currentChild.Name + " is expected to be a CPUParticles3D, GPUParticles3D or AnimationPlayer.");
				}
			}
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

	// track particles as they finish and emit a signal when all are completed
	private void AccumulateCompletedParticles()
	{
		_completedParticleCount += 1;

		if (_completedParticleCount == _particles.Count)
		{
			_completedParticleCount = 0;
			EmitSignal(SignalName.Finished);
			Playing = false;
		}
	}
}
