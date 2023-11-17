using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class ImpactAudioSettings : Resource
{
    [Export] public Array<AudioStream> ImpactSoundsQuiet;
    [Export] public Array<AudioStream> ImpactSoundsLoud;
}
