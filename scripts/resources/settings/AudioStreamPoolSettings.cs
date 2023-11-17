using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AudioStreamPoolSettings : Resource
{
    [Export] public Dictionary<string, Array<AudioStream>> StreamCategories = new();
    [Export] public float OffsetTimeout = .025f;
}
