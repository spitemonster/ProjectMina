using Godot;
using System;
using System.Transactions;

namespace ProjectMina;
public partial class SoundPlayer2D : AudioStreamPlayer2D
{
    [Signal] public delegate void PlayedEventHandler();
    [Signal] public delegate void PausedEventHandler();
    [Signal] public delegate void StoppedEventHandler(float timeRemaining);
    
    [Export] public bool Loop = false;

    private float _pausedPlaybackTime;
    
    new public void Play(float fromPosition = 0.0f)
    {
        EmitSignal(SignalName.Played);

        if (StreamPaused && fromPosition == 0.0f)
        {
            base.Play(_pausedPlaybackTime);
            _pausedPlaybackTime = 0.0f;
        }
        else
        {
            base.Play(fromPosition);
        }
    }

    public void Pause()
    {
        if (!StreamPaused)
        {
            _pausedPlaybackTime = GetPlaybackPosition();
            StreamPaused = true;
            EmitSignal(SignalName.Paused);
        }
    }

    new public void Stop()
    {
        EmitSignal(SignalName.Stopped);
        base.Stop();
    }
    
    public override void _Ready()
    {
        Finished += () =>
        {
            if (Loop)
            {
                Play();
            }
        };
    }
}
