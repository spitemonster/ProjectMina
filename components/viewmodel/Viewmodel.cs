using Godot;

namespace ProjectMina;
public partial class Viewmodel : Node3D
{
    private Camera3D _camera;
    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("%Camera");
    }
    
    public override void _Process(double delta)
    {
        _camera.GlobalTransform = GlobalTransform;
    }
}
