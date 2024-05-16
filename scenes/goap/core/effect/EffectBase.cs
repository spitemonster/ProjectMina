using Godot;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class EffectBase : GodotObject
{
    public StringName Property;
    public bool Increase = false;
}