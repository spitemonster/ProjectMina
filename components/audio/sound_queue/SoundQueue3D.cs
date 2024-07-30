using Godot;
using Godot.Collections;

namespace ProjectMina;
public partial class SoundQueue3D : Node3D
{
	[Export] public int PlayerCount = 16;

	private Array<SoundPlayer3D> _streamPlayers = new();
	private int _currentStreamPlayerIndex;
	
	public override void _Ready()
	{
		if (GetChild(0) is SoundPlayer3D basePlayer)
		{
			for (var i = 0; i < PlayerCount; i++)
			{
				var clone = basePlayer.Duplicate() as SoundPlayer3D;
				AddChild(clone);
				_streamPlayers.Add(clone);
			}
		}
	}

	public void PlaySound(AudioStream stream)
	{
		var player = _streamPlayers[_currentStreamPlayerIndex];
		player.Stream = stream;
		player.Play();
		_currentStreamPlayerIndex++;
		_currentStreamPlayerIndex %= _streamPlayers.Count;
	}

	public void Stop()
	{
		
	}
}
