using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class AudioStreamPool : PoolBase
{
    public static AudioStreamPool Manager { get; private set; }
    private AudioStreamPoolSettings _settings;
    
    public AudioStream GetRandomStreamFromCategory(string category)
    {
        if (_settings == null)
        {
            if (EnableDebug) GD.PushError("AudioStreamPool missing settings.");
            return null;
        }
        
        if (_settings.StreamCategories.TryGetValue(category, out var streams))
        {
            return streams.Count switch
            {
                0 => null,
                1 => streams[0],
                _ => streams.PickRandom()
            };
        }
        
        if (EnableDebug) GD.PushError("Missing stream!");
        return null;
    }

    public override void _Ready()
    {
        EnableDebug = true;
        _settings = ResourceLoader.Load<AudioStreamPoolSettings>("res://resources/settings/AudioStreamPoolSettings.tres");

        if (_settings == null && EnableDebug)
        {
            GD.PushError("AudioStreamPool doesn't have settings");
        }
    }
    
    public override void _EnterTree()
    {
        if (Manager != null)
        {
            QueueFree();
        }
        Manager = this;
    }
}
