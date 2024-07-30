using Godot;

namespace ProjectMina;

public enum EFaction : uint
{
    Player,
    FactionA,
    FactionB,
    NumFactions,
    None
}

[GlobalClass]
public partial class FactionComponent : Node
{
    [Export] public EFaction Faction { get; protected set; }= EFaction.FactionA;
}
