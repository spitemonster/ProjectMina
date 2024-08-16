using Godot;

namespace ProjectMina;
public partial class SoundPlayer3D : AudioStreamPlayer3D
{
    [Signal] public delegate void PlayerFinishedEventHandler(SoundPlayer3D player);
    [Signal] public delegate void PlayedEventHandler();
    [Signal] public delegate void PausedEventHandler();
    [Signal] public delegate void StoppedEventHandler(float timeRemaining);

    [Export] public bool Loop = false;

    [Export] public bool FreeOnFinish = false;

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
            } else if (FreeOnFinish)
            {
                QueueFree();
            }
            else
            {
                EmitSignal(SignalName.PlayerFinished, this);
            }
        };
    }
}
