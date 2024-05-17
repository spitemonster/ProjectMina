using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public enum PerceptionType : int
{
    None,
    Sight,
    Hearing,
    All
}
public partial class PerceptionContext : GodotObject
{
    public PerceptionContext(CharacterBase character, Vector3 position, PerceptionType type, float distance, bool characterVisible, bool characterInFieldOfView, bool characterInPerceptionRadius)
    {
        Type = type;
        Distance = distance;
        Character = character;
        CharacterVisible = characterVisible;
        CharacterInFieldOfView = characterInFieldOfView;
        CharacterInPerceptionRadius = characterInPerceptionRadius;
    }
    
    public CharacterBase Character { get; private set; }
    public PerceptionType Type { get; private set; }
    public Vector3 Position { get; private set; }
    public float Distance { get; private set; }
    public bool CharacterVisible { get; private set; }
    public bool CharacterInFieldOfView { get; private set; }
    public bool CharacterInPerceptionRadius { get; private set; }
}

[GlobalClass]
public partial class AIPerceptionComponent : ComponentBase
{
    // characters within perception radius
    public Array<CharacterBase> CharactersInPerceptionRadius { get; private set; } = new();

    // characters within perception radius and within the character's field of view, does not do line of sight test
    public Array<CharacterBase> CharactersInFieldOfView { get; private set; } = new();

    // sightable characters that pass line of sight test
    public Array<CharacterBase> VisibleCharacters { get; private set; } = new();

    [ExportCategory("Perception")] [Export]
    public Area3D PerceptionRadius;

    [ExportCategory("Vision")] [Export] protected bool EnableSight = true;
    [Export] public float FOVAngle { get; protected set; } = 165.0f;
    [Export] public float MaxSightDistance = 10.0f;
    [Export] public Curve VisibilityDistanceCurve { get; protected set; }
    [Export] public Curve VisibilityHorizontalCurve { get; protected set; }
    [Export] public Curve VisibilityVerticalCurve { get; protected set; }

    [Signal] public delegate void PlayerEnteredLineOfSightEventHandler();

    [Signal] public delegate void PlayerNoticedEventHandler();

    [Signal] public delegate void PlayerExitedLineOfSightEventHandler();

    [Signal] public delegate void CharacterEnteredPerceptionRadiusEventHandler(CharacterBase character);

    [Signal] public delegate void CharacterExitedPerceptionRadiusEventHandler(CharacterBase character);

    [Signal] public delegate void CharacterEnteredFieldOfViewEventHandler(CharacterBase character);

    [Signal] public delegate void CharacterExitedFieldOfViewEventHandler(CharacterBase character);

    [Signal] public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);

    [Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);

    [Signal] public delegate void SoundHeardEventHandler(SoundSourceComponent source);
    // [Signal] public delegate void CharacterPerceivedEventHandler(PerceptionContext context);

    [Signal] public delegate void PerceptionUpdatedEventHandler(PerceptionContext context);

    [ExportCategory("Hearing")] [Export] protected bool EnableHearing = true;

    [ExportCategory("Debug")] [Export] protected ProgressBar EnemyVisibilityLevelIndicator;
    [Export] protected ProgressBar EnemyAwarenessLevelIndicator;

    [Export] protected float VisibilityDistanceWeight = 1.0f;
    [Export] protected float VisibilityHorizontalWeight = 1.0f;
    [Export] protected float VisibilityVerticalWeight = .5f;
    [Export] protected float VisibilityLightednessWeight = .8f;

    [Export] protected float PerceptionRate = .1f;
    [Export] protected bool EnableClosePerception = true;
    [Export] protected bool EnableFarPerception = true;

    private CharacterBase _owner;

    public Timer _perceptionTimer;

    private float _awareness;

    private PlayerCharacter _targetCharacter;
    private bool _targetVisible = false;

    
    private float _maxDotProduct;

    public CharacterBase GetNearestVisibleCharacter()
    {
        // return null if we don't have any visible characters
        if (VisibleCharacters.Count < 1)
        {
            return null;
        }
        
        VisibleCharacters =
            new Array<CharacterBase>(
                VisibleCharacters.OrderByDescending(c => (c.GlobalPosition - _owner.GlobalPosition).Length()));

        return VisibleCharacters[0];
    }

    public override void _Ready()
    {
        base._Ready();
        
        _maxDotProduct = Mathf.Cos(Mathf.DegToRad(FOVAngle / 2.0f));

        _owner = GetOwner<CharacterBase>();

        _perceptionTimer = new Timer()
        {
            WaitTime = PerceptionRate,
            Autostart = false,
            OneShot = false
        };

        GetTree().Root.AddChild(_perceptionTimer);
        _perceptionTimer.Timeout += _Perceive;
        

        PerceptionRadius.BodyEntered += _CheckAddBody;
        PerceptionRadius.BodyExited += _CheckRemoveBody;

        CallDeferred("_CheckInitialOverlaps");
    }

    private void _CheckInitialOverlaps()
    {
        foreach (var body in PerceptionRadius.GetOverlappingBodies())
        {
            _CheckAddBody(body);
        }
        
        _perceptionTimer.Start();
    }

    private void _Perceive()
    {
        var forwardVector = _owner.ForwardVector;
        // for each character that we could possibly perceive, we want to determine if they are in our field of view (thus sightable)
        
        foreach (var character in CharactersInPerceptionRadius)
        {
            // dot product test to determine if character is in field of view
            

            if (_IsInFieldOfView(character.Chest.GlobalPosition))
            { 
                _AddSightableCharacter(character);
                
                if (_owner.HasLineOfSight(character.Chest))
                {
                    _AddVisibleCharacter(character);
                }
                else
                {
                    _RemoveVisibleCharacter(character);
                }
            }
            else
            {
                _RemoveSightableCharacter(character);
            }
        }
    }

    private void _CheckAddBody(Node3D body)
    {
        // don't worry about this body if we're already perceiving it, if it's our owner or it's not a character
        if (body is not CharacterBase character || character == _owner ||
            CharactersInPerceptionRadius.Contains(character)) return;
        
        // potentially temporary; only perceive the player character
        if (character is not PlayerCharacter pc)
        {
            return;
        }
        
        _AddPerceivableCharacter(character);
    }

    private void _CheckRemoveBody(Node3D body)
    {
        if (body is CharacterBase character && CharactersInPerceptionRadius.Contains(character))
        {
            _RemovePerceivableCharacter(character);
        }
    }
    
    private void _AddPerceivableCharacter(CharacterBase character)
    {
        CharactersInPerceptionRadius.Add(character);
        EmitSignal(SignalName.CharacterEnteredPerceptionRadius, character);

        var pos = character.GlobalPosition;
        var dist = _owner.GlobalPosition.DistanceTo(pos);
        var hasLos = _owner.HasLineOfSight(character);
        var inFov = _IsInFieldOfView(character.Chest.GlobalPosition);
        
        PerceptionContext context = new(character, pos, PerceptionType.All, dist, hasLos, inFov, true);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }

    private void _RemovePerceivableCharacter(CharacterBase character)
    {
        CharactersInPerceptionRadius.Remove(character);
        // this sort of bubbles up; if a character is no longer perceivable it cannot be sightable OR visible
        // but the reverse can be the case; we can lose sight of the character with them still in the perception radius
        // so these functions will always call the one above them but never the one below
        _RemoveSightableCharacter(character);

        EmitSignal(SignalName.CharacterExitedPerceptionRadius, character);
        
        PerceptionContext context = new(character, Vector3.Zero, PerceptionType.None, 0, false, false, false);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }
    
    private void _AddSightableCharacter(CharacterBase character)
    {
        if (CharactersInFieldOfView.Contains(character))
        {
            return;
        }
        
        CharactersInFieldOfView.Add(character);
        EmitSignal(SignalName.CharacterEnteredFieldOfView, character);
        
        var pos = character.GlobalPosition;
        var dist = _owner.GlobalPosition.DistanceTo(pos);
        var hasLos = _owner.HasLineOfSight(character);
        
        PerceptionContext context = new(character, Vector3.Zero, PerceptionType.Sight, dist, hasLos, true, true);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }
    
    private void _RemoveSightableCharacter(CharacterBase character)
    {
        // don't run if the character isn't in this array
        if (!CharactersInFieldOfView.Contains(character))
        {
            return;
        }
        
        CharactersInFieldOfView.Remove(character);
        _RemoveVisibleCharacter(character);
        EmitSignal(SignalName.CharacterExitedFieldOfView, character);

        var inRadius = CharactersInPerceptionRadius.Contains(character);
        
        PerceptionContext context = new(character, Vector3.Zero, PerceptionType.Sight, 0, false, false, inRadius);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }
    
    private void _AddVisibleCharacter(CharacterBase character)
    {
        if (VisibleCharacters.Contains(character))
        {
            return;
        }
        
        VisibleCharacters.Add(character);
        EmitSignal(SignalName.CharacterEnteredLineOfSight, character);
        
        var pos = character.GlobalPosition;
        var dist = _owner.GlobalPosition.DistanceTo(pos);
        
        PerceptionContext context = new(character, pos, PerceptionType.Sight, dist, true, true, true);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }

    private void _RemoveVisibleCharacter(CharacterBase character)
    {
        if (!VisibleCharacters.Contains(character))
        {
            return;
        }

        VisibleCharacters.Remove(character);
        EmitSignal(SignalName.CharacterExitedLineOfSight, character);
        
        var sightable = CharactersInFieldOfView.Contains(character);
        var inRadius = CharactersInPerceptionRadius.Contains(character);
        
        PerceptionContext context = new(character, Vector3.Zero, PerceptionType.Sight, 0, false, sightable, inRadius);
        EmitSignal(SignalName.PerceptionUpdated, context);
    }

    private bool _IsInFieldOfView(Vector3 targetPosition)
    {
        var toTarget = (targetPosition - _owner.Eyes.GlobalPosition).Normalized();
        var dotProduct = _owner.ForwardVector.Dot(toTarget);
        // Check if the dot product is within the allowed range
        return dotProduct >= _maxDotProduct;
    }
}