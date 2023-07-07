using System.Data.Common;
using Godot;
using ProjectMina;

namespace ProjectMina;
public partial class DevUI : Control
{
	private Label _FPSLabel;
	private Label _maxFPSLabel;
	private Label _minFPSLabel;
	private VBoxContainer _notificationQueue;
	private VBoxContainer _monitorContainer;
	private double _minFPS = 120.0;
	private double _maxFPS = 0.0;
	private bool _minFPSTimeoutEnded = false;
	private PackedScene ui;

	public override void _Ready()
	{
		base._Ready();

		Global.Data.DevLog = this;
		_FPSLabel = GetNode<Label>("%FPS");
		_maxFPSLabel = GetNode<Label>("%MaxFPS");
		_minFPSLabel = GetNode<Label>("%MinFPS");
		_monitorContainer = GetNode<VBoxContainer>("%MonitorContainer");
		_notificationQueue = GetNode<VBoxContainer>("%NotificationQueue");
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
	}

	public void PushDevNotification(string notification)
	{
		Growler msg = new()
		{
			Text = notification
		};

		_notificationQueue.AddChild(msg);
	}
}
