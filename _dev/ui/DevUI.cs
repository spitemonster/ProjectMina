using System.Data.Common;
using Godot;
using ProjectMina;

namespace ProjectMina;
public partial class DevUI : Control
{
	private Label _FPSLabel;
	private Label _maxFPSLabel;
	private Label _minFPSLabel;
	private Label _frameTimeLabel;
	private VBoxContainer _notificationQueue;
	private VBoxContainer _monitorContainer;
	private double _minFPS = 120.0;
	private double _maxFPS = 0.0;
	private bool _minFPSTimeoutEnded = false;
	private PackedScene ui;
	private int _lastuSec = 0;

	public override void _Ready()
	{
		base._Ready();

		Global.Data.DevLog = this;
		_FPSLabel = GetNode<Label>("%FPS");
		_maxFPSLabel = GetNode<Label>("%MaxFPS");
		_minFPSLabel = GetNode<Label>("%MinFPS");
		_frameTimeLabel = GetNode<Label>("%FrameTime");
		_monitorContainer = GetNode<VBoxContainer>("%MonitorContainer");
		_notificationQueue = GetNode<VBoxContainer>("%NotificationQueue");

		Timer minFPSTimer = new()
		{
			OneShot = true,
			Autostart = true,
			WaitTime = .75f
		};

		minFPSTimer.Timeout += () =>
		{
			_minFPSTimeoutEnded = true;
			minFPSTimer.QueueFree();
		};

		AddChild(minFPSTimer);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		double currentFPS = Engine.GetFramesPerSecond();

		_FPSLabel.Text = currentFPS.ToString();

		if (currentFPS < _minFPS && _minFPSTimeoutEnded)
		{
			_minFPS = currentFPS;
			_minFPSLabel.Text = _minFPS.ToString();
		}

		if (currentFPS > _maxFPS)
		{
			_maxFPS = currentFPS;
			_maxFPSLabel.Text = _maxFPS.ToString();
		}

		_frameTimeLabel.Text = (Mathf.Round(Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000) / 100).ToString();
	}

	public void PushDevNotification(string notification)
	{
		Growler msg = new()
		{
			Text = notification
		};

		_notificationQueue.AddChild(msg);
	}

	public LabelValueRow AddDevMonitor(string label)
	{
		LabelValueRow newMonitor = new();
		_monitorContainer.AddChild(newMonitor);
		newMonitor.DisplayLabel = label;

		return newMonitor;
	}
}
