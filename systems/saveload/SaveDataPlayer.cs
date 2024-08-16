using Godot;

namespace ProjectMina;

public partial class SaveDataPlayer : SaveDataCharacter
{
    [Export] public Transform3D CameraTransform = new();
}