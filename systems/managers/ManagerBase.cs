using Godot;
using System;

namespace ProjectMina;
    
[GlobalClass]
public partial class ManagerBase : Node
{
    [ExportGroup("ManagerBase")]
    [Export] protected bool Enabled = true;

    public override void _Ready()
    {
        SetProcess(Enabled);
    }
}
