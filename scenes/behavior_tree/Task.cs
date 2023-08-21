using System;
using Godot;

[GlobalClass]
public partial class Task : Node
{
	public enum TaskStatus
	{
		FRESH,
		RUNNING,
		FAILED,
		SUCCEEDED,
		CANCELED
	}

	public TaskStatus Status = TaskStatus.FRESH;
	public Node Control;
	public Node Tree;
	public Node Guard;

	[Signal] public delegate void StatusChangedEventHandler(TaskStatus newState);

	public void Run()
	{
		Status = TaskStatus.RUNNING;
	}

	public void Succeed()
	{

	}

	public void Fail()
	{

	}

	public void Cancel()
	{

	}
}
