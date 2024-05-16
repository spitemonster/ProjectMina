using Godot;

namespace ProjectMina;

public partial class SaveDataCharacter : SaveDataBase
{
    [Export] public Vector3 Position = new();
    [Export] public Transform3D Transform = new();
} 