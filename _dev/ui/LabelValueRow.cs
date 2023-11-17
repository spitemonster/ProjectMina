using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class LabelValueRow : HBoxContainer
{

	public string DisplayLabel
	{
		get => _displayLabel;
		set
		{
			_labelNode.Text = value;
			_displayLabel = value;
		}
	}

	[Export]
	protected string _displayLabel;

	private Label _labelNode;
	private Label _valueNode;

	public override void _EnterTree()
	{
		base._EnterTree();

		_labelNode = new()
		{
			Text = _displayLabel
		};

		AddChild(_labelNode);

		_valueNode = new();

		AddChild(_valueNode);
	}

	public override void _Ready()
	{

		System.Diagnostics.Debug.Assert(_labelNode != null, "There is no label node!");
		// base._Ready();

		// if (GetNode("%Label") is Label l)
		// {
		// 	_labelNode = l;
		// }
		// _valueNode = GetNode<Label>("%Value");

		// _labelNode.Text = _displayLabel;
	}

	public void SetValue(string value)
	{
		_valueNode.Text = value;
	}
}
