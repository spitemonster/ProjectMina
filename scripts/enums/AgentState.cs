using Godot;
namespace ProjectMina;

public enum EAgentState : uint
{
    Idle,
    Patrol,
    Suspicious,
    Combat
}