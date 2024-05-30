using Godot;

namespace ProjectMina.EnvironmentQuerySystem;

[Tool]
[GlobalClass]
public partial class EQSNode : Node
{
    [Export] public bool Debug { get; protected set; } = true;
}
