using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

public partial class Wait : Action
{
	[Export] public float WaitTime;

	private bool _timerStarted = false;
	private bool _timerFinished = false;
	
	private Timer _waitTimer;

	public override void _Ready()
	{
		base._Ready();
		
		_waitTimer = new()
		{
			WaitTime = WaitTime,
			Autostart = false,
			OneShot = false
		};

		_waitTimer.Timeout += () =>
		{
			_timerFinished = true;
		};
		
		GetTree().Root.AddChild(_waitTimer);
	}
	
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		if (!_timerStarted)
		{
			Dev.UI.PushDevNotification("am start wait timer");
			_waitTimer.Start();
			_timerStarted = true;
			_timerFinished = false;
			SetStatus(EActionStatus.Running);
			return Status;
		}

		if (!_timerFinished)
		{
			SetStatus(EActionStatus.Running);
			return Status;
		}
		
		_timerStarted = false;
		SetStatus(EActionStatus.Succeeded);
		return Status;
	}
}
