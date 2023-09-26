using Godot;
using Godot.Collections;
using System.Diagnostics;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class SoundQueue3D : Node3D
{
	[Export] protected int PlayerCount = 16;

	private Godot.Collections.Array<AudioStreamPlayer3D> _streamPlayers = new();
	private int _currentStreamPlayerIndex = 0;

	public override void _Ready()
	{
		if (GetChild(0) is AudioStreamPlayer3D basePlayer)
		{
			for (int i = 0; i < PlayerCount; i++)
			{
				AudioStreamPlayer3D clone = basePlayer.Duplicate() as AudioStreamPlayer3D;
				AddChild(clone);
				_streamPlayers.Add(clone);
			}
		}
	}

	public void PlaySound(AudioStream stream)
	{
		AudioStreamPlayer3D player = _streamPlayers[_currentStreamPlayerIndex];
		player.Stream = stream;
		player.Play();
		_currentStreamPlayerIndex++;
		_currentStreamPlayerIndex %= _streamPlayers.Count;
	}

	public override string[] _GetConfigurationWarnings()
	{
		base._GetConfigurationWarnings();
		Array<string> warnings = new();

		if (GetChildCount() != 1)
		{
			warnings.Add("Sound Queue should have exactly one child.");
		}

		if (GetChild(0) is not AudioStreamPlayer3D)
		{
			warnings.Add("Sound Queue should have only an AudioStreamPlayer3D as its child.");
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
