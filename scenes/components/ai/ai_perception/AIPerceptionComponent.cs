using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class AIPerceptionComponent : ComponentBase
{
    // characters within perception radius
    public Array<CharacterBase> CharactersInPerceptionRadius = new();

    // characters within perception radius and within the character's field of view, does not do line of sight test
    public Array<CharacterBase> SightableCharacters = new();

    // sightable characters that pass line of sight test
    public Array<CharacterBase> VisibleCharacters = new();

    public CharacterBase VisibleCharacter;

    [ExportCategory("Perception")] [Export]
    public Area3D PerceptionRadius;

    [Export] protected float PerceptionRate = .1f;
    [Export] protected bool EnableClosePerception = true;
    [Export] protected bool EnableFarPerception = true;

    [ExportCategory("Vision")] [Export] protected bool EnableSight = true;
    [Export] public float MaxSightDistance = 10.0f;
    [Export] public Curve VisibilityDistanceCurve { get; protected set; }
    [Export] public Curve VisibilityHorizontalCurve { get; protected set; }
    [Export] public Curve VisibilityVerticalCurve { get; protected set; }
    [Export] protected float VisibilityDistanceWeight = 1.0f;
    [Export] protected float VisibilityHorizontalWeight = 1.0f;
    [Export] protected float VisibilityVerticalWeight = .5f;
    [Export] protected float VisibilityLightednessWeight = .8f;

    [ExportCategory("Hearing")] [Export] protected bool EnableHearing = true;

    [ExportCategory("Debug")] [Export] protected ProgressBar EnemyVisibilityLevelIndicator;
    [Export] protected ProgressBar EnemyAwarenessLevelIndicator;

    [Signal] public delegate void PlayerEnteredLineOfSightEventHandler();
    [Signal] public delegate void PlayerNoticedEventHandler();
    [Signal] public delegate void PlayerExitedLineOfSightEventHandler();
    [Signal] public delegate void CharacterEnteredPerceptionRadiusEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterExitedPerceptionRadiusEventHandler(CharacterBase character);

    [Signal]
    public delegate void SoundHeardEventHandler(SoundSourceComponent source);

    private Accumulator noticeAccumulator;

    private CharacterBase _owner;

    public Timer _perceptionTimer;

    private float _awareness;

    // we reuse these variables in _physicsprocess
    private Vector3 _toTarget;
    private float _dotProduct;
    private float _angle;
    private float _roundedAngle;
    private float _clampedRange;

    private PlayerCharacter _targetCharacter;
    private bool _targetVisible = false;

    public CharacterBase GetNearestVisibleCharacter()
    {
        VisibleCharacters =
            new Array<CharacterBase>(
                VisibleCharacters.OrderByDescending(c => (c.GlobalPosition - _owner.GlobalPosition).Length()));

        return VisibleCharacters[0];
    }

    public override void _Ready()
    {
        base._Ready();

        _owner = GetOwner<CharacterBase>();

        noticeAccumulator = new Accumulator()
        {
            Threshold = 50
        };

        AddChild(noticeAccumulator);

        // if (_owner == null || PerceptionRadius == null)
        // {
        //     return;
        // }

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
    }

    private void _Perceive()
    {
    }

    private void _CheckAddBody(Node3D body)
    {
        if (body is CharacterBase character && character != _owner && !CharactersInPerceptionRadius.Contains(character))
        {
            // TODO: Add code to determine if we even care to perceive this character
            _AddPerceivableCharacter(character);
        }
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
        if (_targetCharacter == null && character is PlayerCharacter p)
        {
            _targetCharacter = p;
        }
    }

    private void _RemovePerceivableCharacter(CharacterBase character)
    {
        CharactersInPerceptionRadius.Remove(character);
        EmitSignal(SignalName.CharacterExitedPerceptionRadius, character);

        if (_targetCharacter == character)
        {
            _targetCharacter = null;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_targetCharacter == null)
        {
            return;
        }

        var targetVisibility = 0.0f;
        var visible = _owner.HasLineOfSight(_targetCharacter.Chest);

        if (!visible)
        {
            if (_targetVisible)
            {
                _LosePlayer();    
            }
        } else
        {
            if (!_targetVisible)
            {
                _SpotPlayer();
            }

            _toTarget = (_targetCharacter.Chest.GlobalPosition - _owner.Eyes.GlobalPosition).Normalized();
            
            var toTargetHorizontal = new Vector3(_toTarget.X, 0, _toTarget.Z).Normalized();
            var dotProductHorizontal = Mathf.Clamp(_owner.ForwardVector.Dot(toTargetHorizontal), 0, 1);
            var dotProductVertical = Mathf.Abs(_toTarget.Y);
            
            var horizontalAngleComponent = dotProductHorizontal;
            var verticalAngleComponent = 1 - dotProductVertical;

            if (VisibilityHorizontalCurve != null)
            {
                horizontalAngleComponent = VisibilityHorizontalCurve.SampleBaked(dotProductHorizontal);
            }

            if (VisibilityVerticalCurve != null)
            {
                verticalAngleComponent = VisibilityVerticalCurve.SampleBaked(dotProductVertical);
            }

            var dist = (_targetCharacter.GlobalPosition - _owner.GlobalPosition).Length();
            var distRatio = Mathf.Clamp(1 - (dist / MaxSightDistance), 0, 1);

            var distanceComponent = distRatio;

            if (VisibilityDistanceCurve != null)
            {
                distanceComponent = VisibilityDistanceCurve.SampleBaked(distRatio);
            }

            var playerVisibilityComponent = _targetCharacter.GetVisibility();

            targetVisibility = (horizontalAngleComponent * VisibilityHorizontalWeight) *
                                   (verticalAngleComponent * VisibilityVerticalWeight) *
                                   (distanceComponent * VisibilityDistanceWeight);
            targetVisibility -= (1 - playerVisibilityComponent) * VisibilityLightednessWeight;
            targetVisibility = Mathf.Max(0, targetVisibility);
        }
        
        // GD.Print("target visibility: ", targetVisibility);
        
        // noticeAccumulator.Value = targetVisibility;
    }

    private void _SpotPlayer()
    {
        GD.Print("player spotted");
        _CharacterEnterLineOfSight();
    }

    private void _LosePlayer()
    {
        GD.Print("lost sight of player");
        _CharacterExitLineOfSight();
    }

    private void _CharacterEnterLineOfSight()
    {
        _targetVisible = true;
        EmitSignal(SignalName.PlayerEnteredLineOfSight);
    }

    private void _CharacterExitLineOfSight()
    {
        _targetVisible = false;
        EmitSignal(SignalName.PlayerExitedLineOfSight);
    }
    //
    // private void _AddSightableCharacter(CharacterBase character)
    // {
    //     SightableCharacters.Add(character);
    //     EmitSignal(SignalName.CharacterEnteredFieldOfView, character);
    //     // GD.Print("added sighted character: ", character);
    // }
    //
    // private void _RemoveSightableCharacter(CharacterBase character)
    // {
    //     SightableCharacters.Remove(character);
    //     EmitSignal(SignalName.CharacterExitedFieldOfView, character);
    //     // GD.Print("removed sighted character: ", character);
    //
    //     if (VisibleCharacters.Contains(character))
    //     {
    //         _RemoveVisibleCharacter(character);
    //     }
    // }
    //
    // private void _AddVisibleCharacter(CharacterBase character)
    // {
    //     VisibleCharacters.Add(character);
    //     EmitSignal(SignalName.CharacterEnteredLineOfSight, character);
    //     // GD.Print("added visible character: ", character);
    // }
    //
    // private void _RemoveVisibleCharacter(CharacterBase character)
    // {
    //     VisibleCharacters.Remove(character);
    //     EmitSignal(SignalName.CharacterExitedLineOfSight, character);
    //
    //     if (VisibleCharacter == character)
    //     {
    //         VisibleCharacter = null;
    //     }
    //     // GD.Print("removed visible character: ", character);
    // }
    //
    // public override void _PhysicsProcess(double delta)
    // {
    //     var forwardVector = _owner.ForwardVector;
    //     
    //     foreach (var character in CharactersInPerceptionRadius)
    //     {
    //         _toTarget = (character.Chest.GlobalPosition - _owner.Eyes.GlobalPosition).Normalized();
    //         _dotProduct = forwardVector.Dot(_toTarget);
    //         _angle = Mathf.Acos(_dotProduct) * (180.0f / Mathf.Pi);
    //         _roundedAngle = Mathf.Clamp(Mathf.Round(_angle), -90, 90);
    //         _clampedRange = Mathf.Clamp(1 - Mathf.Round((_roundedAngle / 90) * 100) / 100, 0, 1);
    //         
    //         if (_clampedRange > 0 && !SightableCharacters.Contains(character))
    //         {
    //             _AddSightableCharacter(character);
    //         } else if (_clampedRange <= 0 && SightableCharacters.Contains(character))
    //         {
    //             _RemoveSightableCharacter(character);
    //         }
    //     }
    //
    //     foreach (var character in SightableCharacters)
    //     {
    //         if (_owner.HasLineOfSight(character.Chest))
    //         {
    //             if (!VisibleCharacters.Contains(character))
    //             {
    //                 _AddVisibleCharacter(character);
    //             }
    //         } else if (VisibleCharacters.Contains(character))
    //         {
    //             GD.Print("should lose line of sight");
    //             _RemoveVisibleCharacter(character);
    //         }
    //     }
    //
    //     if (VisibleCharacter != null)
    //     {
    //         _toTarget = (VisibleCharacter.Chest.GlobalPosition - _owner.Eyes.GlobalPosition).Normalized();
    //         var toTargetHorizontal = new Vector3(_toTarget.X, 0, _toTarget.Z).Normalized();
    //         var dotProductHorizontal = Mathf.Clamp(forwardVector.Dot(toTargetHorizontal), 0, 1);
    //         var dotProductVertical = Mathf.Abs(_toTarget.Y);
    //
    //         var horizontalAngleComponent = dotProductHorizontal;
    //         var verticalAngleComponent = 1 - dotProductVertical;
    //
    //         if (VisibilityHorizontalCurve != null)
    //         {
    //             horizontalAngleComponent = VisibilityHorizontalCurve.SampleBaked(dotProductHorizontal);
    //         }
    //         
    //         // GD.Print("horizontal component: ", horizontalAngleComponent);
    //
    //         if (VisibilityVerticalCurve != null)
    //         {
    //             verticalAngleComponent = VisibilityVerticalCurve.SampleBaked(dotProductVertical);
    //         }
    //         
    //         // GD.Print("vertical component: ", verticalAngleComponent);
    //         
    //         var dist = (VisibleCharacter.GlobalPosition - _owner.GlobalPosition).Length();
    //         var distRatio = Mathf.Clamp(1 - (dist / MaxSightDistance), 0, 1);
    //         
    //         var distanceComponent = distRatio;
    //
    //         if (VisibilityDistanceCurve != null)
    //         {
    //             distanceComponent = VisibilityDistanceCurve.SampleBaked(distRatio);
    //         }
    //         
    //         // GD.Print("distance component: ", distanceComponent);
    //
    //         if (VisibleCharacter is PlayerCharacter p)
    //         {
    //             // GD.Print("visibility component: ", p.GetVisibility());
    //         }
    //         
    //         //
    //         // if (dist <= 1)
    //         // {
    //         //     _clampedRange = 1.0f;
    //         // }
    //         //
    //         // var t = _clampedRange * distVal;
    //         // _awareness += t;
    //         //
    //         // EnemyAwarenessLevelIndicator.Value = _awareness;
    //         // EnemyVisibilityLevelIndicator.Value = t * 100;
    //     }
    //     else
    //     {
    //         if (VisibleCharacters.Count > 0)
    //         {
    //             
    //             VisibleCharacter = VisibleCharacters[0];
    //         }
    //         else
    //         {
    //             // _awareness -= 1f;
    //             EnemyVisibilityLevelIndicator.Value = 0;
    //             EnemyAwarenessLevelIndicator.Value = 0;
    //         }
    //     }
    // }
}