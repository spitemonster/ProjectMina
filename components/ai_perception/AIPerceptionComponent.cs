using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class AIPerceptionComponent : ComponentBase
{

    [Signal] public delegate void CharacterNoticedEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterSeenEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);
    [Signal] public delegate void CharacterLostVisualDetectionEventHandler(CharacterBase character);

    [ExportGroup("Perception")]
    [Export(PropertyHint.Range, "0,1,.1")] public float DetectionUpdateFrequency { get; private set; } = .1f;
    [Export] public bool Enabled = true;
    
    [ExportSubgroup("Sight")] 
    [Export] public AISenseSight SightComponent;
    [Export(PropertyHint.Range, "0,5,")] public float NoticeDelay { get; private set; } = 1.0f;
    [Export] public float DetectionThreshold { get; private set; } = 3.0f;
    [Export] public float DetectionAccumulationRate { get; private set; } = .1f;
    [Export] public float DetectionReductionRate { get; private set; } = .3f;
    
    private Timer _noticeTimer;
    private CharacterBase _sightTarget = null;
    private Timer _detectionTimer;
    private float _detectionLevel = 0.0f;

    private bool _sightTargetNoticed = false;
    private bool _sightTargetLineOfSight = false;

    private AICharacter _aiOwner;

    public CharacterBase GetNearestVisibleCharacter()
    {
        return null;
    }

    public override void _Ready()
    {
        if (!Enabled)
        {
            return;
        }

        base._Ready();

        _aiOwner = GetOwner<AICharacter>();

        if (_aiOwner == null)
        {
            GD.PushError("Perception component not owned by AI character");
            return;
        }

        if (SightComponent != null)
        {
            SightComponent.CharacterEnteredLineOfSight += _CharacterEnterLineOfSight;
            SightComponent.CharacterExitedLineOfSight += _CharacterExitLineOfSight;
            
            _noticeTimer = new()
            {
                OneShot = true,
                WaitTime = NoticeDelay,
                Autostart = false
            };
        
            _noticeTimer.Timeout += _NoticeCharacter;
            
            GetTree().Root.AddChild(_noticeTimer);
        }

        _detectionTimer = new()
        {
            OneShot = false,
            WaitTime = DetectionUpdateFrequency,
            Autostart = false
        };
        
        _detectionTimer.Timeout += _UpdateDetection;
        GetTree().Root.AddChild(_detectionTimer);
    }

    private void _UpdateDetection()
    {

        if (!Enabled)
        {
            return;
        }
        
        var agentState = _aiOwner.AIController.AgentState;
        var visibility = 1f;

        if (_sightTarget.GetNodeOrNull("VisibilityComponent") is VisibilityComponent v)
        {
            visibility = v.GetVisibility();
        }
        
        _aiOwner.DetectionLabel.Text = _detectionLevel.ToString();
        if (_sightTarget == null || _sightTargetNoticed == false)
        {
            _EndCharacterDetection();
            return;
        }

        if (_sightTargetLineOfSight)
        {
            _detectionLevel += DetectionAccumulationRate;

            if (_detectionLevel >= DetectionThreshold)
            {
                _SeeCharacter();
                _EndCharacterDetection();
            }
        }
        else if (_detectionLevel > 0f)
        {
            var reductionRate = DetectionReductionRate;

            switch (agentState)
            {
                case EAgentState.Combat:
                    reductionRate *= .1f;
                    break;
                case EAgentState.Suspicious:
                    reductionRate *= .25f;
                    break;
            }
            
            _detectionLevel -= reductionRate;
        }
        else
        {
            _detectionLevel = 0f;
            _EndCharacterDetection();
        }
    }

    private void _CharacterEnterLineOfSight(CharacterBase character)
    {
        if (_sightTarget != null && _sightTargetLineOfSight)
        {
            return;
        }

        _sightTarget = character;
        _noticeTimer.Start();
        _sightTargetLineOfSight = true;
        Dev.UI.PushDevNotification("character entered line of sight: " + character.Name);
        EmitSignal(SignalName.CharacterEnteredLineOfSight, _sightTarget);
    }
    
    private void _CharacterExitLineOfSight(CharacterBase character)
    {
        if (_sightTarget != character)
        {
            return;
        }

        if (!_sightTargetNoticed)
        {
            _sightTarget = null;
            _noticeTimer.Stop();
        }

        if (_sightTargetNoticed)
        {
            _detectionLevel = DetectionThreshold;
            _detectionTimer.Start();
        }
        
        Dev.UI.PushDevNotification("character exited line of sight: " + character.Name);

        _sightTargetLineOfSight = false;
        EmitSignal(SignalName.CharacterExitedLineOfSight, character);
    }
    
    private void _NoticeCharacter()
    {
        if (_sightTarget != null)
        {
            Dev.UI.PushDevNotification("character noticed: " + _sightTarget.Name);
            EmitSignal(SignalName.CharacterNoticed, _sightTarget);
            _sightTargetNoticed = true;
            _StartCharacterDetection();
        }
    }

    private void _SeeCharacter()
    {
        if (_sightTarget == null)
        {
            return;
        }
        
        Dev.UI.PushDevNotification("character seen: " + _sightTarget.Name);
        EmitSignal(SignalName.CharacterSeen, _sightTarget);
    }

    private void _StartCharacterDetection()
    {
        // Dev.UI.PushDevNotification("character entered line of sight: " + character.Name);
        // // don't do anything else if we're already focused on a particular character

        _detectionTimer.Start();
    }

    private void _EndCharacterDetection()
    {
        // TODO: unfuck this
        if (_sightTarget == null)
        {
            return;
        }
        
        _detectionTimer.Stop();

        if (_detectionLevel >= DetectionThreshold)
        {
            return;
        }
        
        EmitSignal(SignalName.CharacterLostVisualDetection, _sightTarget);
        _sightTarget = null;
    }

    // private bool _IsInFieldOfView(Vector3 targetPosition)
    // {
    //     // var toTarget = (targetPosition - _owner.Head.GlobalPosition).Normalized();
    //     // var dotProduct = _owner.ForwardVector.Dot(toTarget);
    //     // // Check if the dot product is within the allowed range
    //     // return dotProduct >= _maxDotProduct;
    //     return false;
    // }
}