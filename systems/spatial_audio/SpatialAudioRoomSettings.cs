using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class SpatialAudioRoomSettings : Resource
{
    [Export] public AudioEffect[] Effects;
}
