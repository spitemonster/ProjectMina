using System.Threading.Tasks;
using Godot;
using Godot.Collections;
namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public abstract partial class Action : Node
{
	[Signal] public delegate void ActionCompletedEventHandler(EActionStatus ActionStatus);
	[Signal] public delegate void ActionStatusChangedEventHandler(EActionStatus newStatus);
	[Export] public bool IsActive { get; private set; } = true;
	// if an action that inherits this class should modify the blackboard directly, such as finding and setting a target location, it can be set here
	[Export] public StringName BlackboardKey { get; protected set; }
	public EActionStatus Status { get; private set; }
	[Export] protected bool _debug;
	protected Array<Action> ChildActions = new();

	[Export] protected Array<Decorator> Decorators = new();

	private Action _rootAction = null;
	private BehaviorTreeComponent _behaviorTree = null;
	private bool _isRoot = false;

	public virtual void SetBehaviorTree(BehaviorTreeComponent behaviorTree)
	{
		_behaviorTree = behaviorTree;
		
		foreach (var action in ChildActions)
		{
			action.SetBehaviorTree(behaviorTree);
		}
	}

	public virtual void SetRootAction(Action rootAction)
	{
		if (rootAction == this)
		{
			_isRoot = true;
		}

		foreach (var action in ChildActions)
		{
			action.SetRootAction(rootAction);
		}
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		
		foreach (Node c in GetChildren())
		{
			if (c is Action a)
			{
				ChildActions.Add(a);
			}
		}
		
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	public EActionStatus Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		if (!IsActive)
		{
			// if action isn't active just skip it and move on
			SetStatus(EActionStatus.Succeeded); 
			return Status;
		}


		EActionStatus result = _Tick(controller, blackboard);
		SetStatus(result);
		return Status;
	}

	protected void SetStatus(EActionStatus newStatus)
	{
		if (Status == newStatus)
		{
			return;
		}

		Status = newStatus;
		EmitSignal(SignalName.ActionStatusChanged, (uint)Status);
	}

	// override this function 
	protected abstract EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard);

	public virtual void Interrupt()
	{
		
	}

	public virtual void BeforeRun()
	{
		
	}

	public virtual void AfterRun()
	{
		
	}
}
