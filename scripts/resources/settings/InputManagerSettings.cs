using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class InputManagerSettings : Resource
{
    [Export] public Array<StringName> HoldableActions = new();
    [Export] public float HoldTimeoutDelay = 0.1f;
    [Export] public float HoldDuration = 1.0f;
}
