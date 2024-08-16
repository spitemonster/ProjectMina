using Godot;

namespace ProjectMina.EQS;

[Tool]
[GlobalClass]
public partial class EQSNode : Node
{
    [Export] public bool EnableDebug { get; protected set; } = true;
}
