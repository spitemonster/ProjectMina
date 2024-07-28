using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class ControllerComponent : ComponentBase
{
    [Signal]
    public delegate void PawnUpdatedEventHandler(CharacterBase newPawn, CharacterBase oldPawn);
    public CharacterBase Pawn;

    public virtual void Possess(CharacterBase pawn)
    {
        CharacterBase previousPawn = UnPossess();
        Pawn = pawn;
        Pawn.SetController(this);
        EmitSignal(SignalName.PawnUpdated, Pawn, previousPawn);
    }

    public virtual CharacterBase UnPossess()
    {
        var previousPawn = Pawn;
        Pawn = null;
        return previousPawn;
    }
}