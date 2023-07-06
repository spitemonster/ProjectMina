using Godot;
using ProjectMina;

namespace ProjectMina;
public partial class DevUI : Control
{

	private Label _FPSLabel;
	private Label _MaxFPSLabel;
	private Label _MinFPSLabel;
	private VBoxContainer _notificationQueue;
	private VBoxContainer _infoWrapper;
	private double _minFPS = 120.0;
	private double _maxFPS = 0.0;

	private bool _minFPSTimeoutEnded = false;

	public override void _Ready()
	{
		base._Ready();

		Global.Data.DevLog = this;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// double currentFPS = Engine.GetFramesPerSecond();

		// _FPSLabel.Text = currentFPS.ToString();

		// if (currentFPS < _minFPS && _minFPSTimeoutEnded)
		// {
		// 	_minFPS = currentFPS;
		// 	_MinFPSLabel.Text = _minFPS.ToString();
		// }

		// if (currentFPS > _maxFPS)
		// {
		// 	_maxFPS = currentFPS;
		// 	_MaxFPSLabel.Text = _maxFPS.ToString();
		// }
	}
}
