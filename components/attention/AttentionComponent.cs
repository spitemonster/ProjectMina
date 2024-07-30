using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class AttentionComponent : ComponentBase
{
	[Signal] public delegate void FocusChangedEventHandler(Node3D newFocus, Node3D previousFocus);
	[Signal] public delegate void FocusGainedEventHandler(Node3D newFocus);
	[Signal] public delegate void FocusLostEventHandler();

	[Export] public ShapeCast3D AttentionCast;
	public Node3D CurrentFocus { get; private set; }

	private CharacterBase _owner;

	public void AddExclude(CharacterBody3D x)
	{
		AttentionCast?.AddException(x);
	}

	public override void _Ready()
	{
		if (AttentionCast == null)
		{
			SetProcess(false);
		}

		_owner = GetOwner<CharacterBase>();
	}
	public override void _Process(double delta)
	{
		if (CurrentFocus != null && CurrentFocus.IsQueuedForDeletion())
		{
			LoseFocus();

			return;
		}
		
		if (!AttentionCast.IsColliding() || AttentionCast.CollisionResult.Count == 0)
		{
			if (CurrentFocus != null)
			{
				LoseFocus();
			}

			return;
		}

		Array<Node3D> colliderResults = new();

		for (var i = 0; i < AttentionCast.CollisionResult.Count; i++)
		{
			if (AttentionCast.GetCollider(i) is Node3D n)
			{
				colliderResults.Add(n);
			}
		}

		if (colliderResults.Count < 1)
		{
			if (CurrentFocus != null)
			{
				LoseFocus();
				
			}

			return;
		}

		if (CurrentFocus != null && colliderResults.Contains(CurrentFocus))
		{
		} else if (CurrentFocus != null && !colliderResults.Contains(CurrentFocus))
		{
			LoseFocus();
		}

		foreach (var node in colliderResults)
		{
			if (!CanFocus(node)) continue;
			
			SetFocus(node);
			break;
		}
	}

	public bool SetFocus(Node3D newFocus)
	{
		// no need to set if they're already the same
		if (CurrentFocus == newFocus)
		{
			return false;
		}

		// if no focus and they're not equal, newFocus isn't null, so emit this
		// this accounts for situations where a player may shift their focus from one item directly to another
		if (CurrentFocus == null)
		{
			EmitSignal(SignalName.FocusGained, newFocus);
		}
		

		EmitSignal(SignalName.FocusChanged, newFocus, CurrentFocus);
		CurrentFocus = newFocus;

		if (CurrentFocus == null)
		{
			return true;
		}

		InteractableComponent interactableComponent = CurrentFocus.GetNodeOrNull<InteractableComponent>("Usable") ?? CurrentFocus.GetNodeOrNull<InteractableComponent>("Equippable");
		
		if (interactableComponent != null)
		{
			interactableComponent.ReceiveFocus(_owner);	
		}
		return true;
	}

	public bool LoseFocus()
	{
		if (CurrentFocus == null)
		{
			return false;
		}

		// use SetFocus to trigger changed event handler
		SetFocus(null);
		EmitSignal(SignalName.FocusLost);
		return true;
	}
	
	private bool CanFocus(Node3D targetObject)
	{
		var distanceToTargetObject = (AttentionCast.GlobalPosition - targetObject.GlobalPosition).LengthSquared();
		return (targetObject.HasNode("Usable") || targetObject.HasNode("Equippable") || targetObject is RigidBody3D) && distanceToTargetObject < 2.0 * 2.0;
	}
}
