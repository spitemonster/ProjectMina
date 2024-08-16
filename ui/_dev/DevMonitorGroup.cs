using Godot;

[Tool]
[GlobalClass]
public partial class DevMonitorGroup : VBoxContainer
{
	[Export] protected string GroupTitle = "Monitor Group";

	private Label _heading;
	
	public override void _Ready()
	{
		_heading = GetNodeOrNull<Label>("%Heading");
		_heading.Text = GroupTitle;
	}

	public void SetTitle(string title)
	{
		GroupTitle = title;
		_heading.Text = GroupTitle;
	}
}
