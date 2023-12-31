using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class AIPerceptionComponent : ComponentBase3D
{
    // characters within perception radius
    public Array<CharacterBase> CharactersInPerceptionRadius = new();
    // characters within perception radius and within the character's field of view, does not do line of sight test
    public Array<CharacterBase> SightableCharacters = new();
    // sightable characters that pass line of sight test
    public Array<CharacterBase> VisibleCharacters = new();
    
    [Export] public Area3D PerceptionRadius;
    
    [Export] protected bool EnableSight = true;
    [Export] protected bool EnableHearing = true;
    [Export] protected float PerceptionRate = .1f;
    [Export] protected bool EnableClosePerception = true;
    [Export] protected bool EnableFarPerception = true;
    
    [Signal] public delegate void CharacterEnteredPerceptionRadiusEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterExitedPerceptionRadiusEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterEnteredFieldOfViewEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterExitedFieldOfViewEventHandler(CharacterBase character);
    
    [Signal] public delegate void SoundHeardEventHandler(SoundSourceComponent source);

    private AICharacter _owner;

    public Timer _perceptionTimer;

    /// <summary>
    /// test if perception component has line of sight to a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    // public bool HasLineOfSight(Vector3 position)
    // {
    //     
    //     HitResult Cast.Ray
    // }

    // we reuse these variables in _physicsprocess
    private Vector3 _toTarget;
    private float _dotProduct;
    private float _angle;
    private float _roundedAngle;
    private float _clampedRange;
    public override void _Ready()
    {
        base._Ready();

        _owner = GetOwner<AICharacter>();

        if (_owner == null || PerceptionRadius == null)
        {
            return;
        }

        _perceptionTimer = new Timer()
        {
            WaitTime = PerceptionRate,
            Autostart = false,
            OneShot = false
        };

        CallDeferred("_Setup");
    }

    private void _Setup()
    {
        GD.Print("perception component set up");
        GetTree().Root.AddChild(_perceptionTimer);
        _perceptionTimer.Timeout += _Perceive;
        PerceptionRadius.BodyEntered += _CheckAddBody;
        PerceptionRadius.BodyExited += _CheckRemoveBody;
    }

    private void _Perceive()
    {
        
    }

    private void _CheckAddBody(Node3D body)
    {
        if (body is CharacterBase character && character != _owner && !CharactersInPerceptionRadius.Contains(character))
        {
            // TODO: Add code to determine if we even care to perceive this character
            _AddPerceivedCharacter(character);
        }
    }

    private void _CheckRemoveBody(Node3D body)
    {
        if (body is CharacterBase character && CharactersInPerceptionRadius.Contains(character))
        {
            _RemovePerceivedCharacter(character);
        }
    }

    private void _AddPerceivedCharacter(CharacterBase character)
    {
        CharactersInPerceptionRadius.Add(character);
        EmitSignal(SignalName.CharacterEnteredPerceptionRadius, character);
        GD.Print("added perceived character: ", character);
    }

    private void _RemovePerceivedCharacter(CharacterBase character)
    {
        CharactersInPerceptionRadius.Remove(character);
        EmitSignal(SignalName.CharacterExitedPerceptionRadius, character);
        GD.Print("removed perceived character: ", character);
    }
    
    private void _AddSightableCharacter(CharacterBase character)
    {
        SightableCharacters.Add(character);
        EmitSignal(SignalName.CharacterEnteredFieldOfView, character);
        GD.Print("added sighted character: ", character);
    }

    private void _RemoveSightableCharacter(CharacterBase character)
    {
        SightableCharacters.Remove(character);
        EmitSignal(SignalName.CharacterExitedFieldOfView, character);
        GD.Print("removed sighted character: ", character);
    }

    public override void _PhysicsProcess(double delta)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var forwardVector = -GlobalBasis.Z;
        
        foreach (var character in CharactersInPerceptionRadius)
        {
            _toTarget = (character.Chest.GlobalPosition - _owner.Eyes.GlobalPosition).Normalized();
            _dotProduct = forwardVector.Dot(_toTarget);
            _angle = Mathf.Acos(_dotProduct) * (180.0f / Mathf.Pi);
            _roundedAngle = Mathf.Clamp(Mathf.Round(_angle), -90, 90);
            _clampedRange = 1 - Mathf.Round((_roundedAngle / 90) * 100) / 100;

            if (_clampedRange > 0 && !SightableCharacters.Contains(character))
            {
                _AddSightableCharacter(character);
            } else if (_clampedRange <= 0 && SightableCharacters.Contains(character))
            {
                _RemoveSightableCharacter(character);
            }
        }
    }
    
    
}