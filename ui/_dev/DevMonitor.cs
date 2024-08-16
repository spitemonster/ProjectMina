using System;
using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class DevMonitor : HBoxContainer
{

	[Export] protected string Label;
	[Export] protected Color ValueColor;
	
	private Label _labelNode;
	private Label _valueNode;
	// private string _labelContent;
	private Color _valueColor;

	public override void _Ready()
	{
		_labelNode = GetNodeOrNull<Label>("%Label");
		_valueNode = GetNodeOrNull<Label>("%Value");
		System.Diagnostics.Debug.Assert(_labelNode != null, "There is no Label node!");
		System.Diagnostics.Debug.Assert(_valueNode != null, "There is no Value node!");

		if (!String.IsNullOrEmpty(Label))
		{
			SetLabel(Label);
		}

		if (ValueColor.A != 0)
		{
			SetValueColor(ValueColor);
		}
	}
	
	public void SetLabel(string value)
	{
		_labelNode.Text = value;
	}

	public void SetValue(string value)
	{
		_valueNode.Text = value;
	}

	public void SetValueColor(Color color)
	{
		_valueNode.Set("theme_override_colors/font_color", color);
	}
}
