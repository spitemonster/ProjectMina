using Godot;

namespace ProjectMina;

public partial class InteractionContext : GodotObject
{
	public bool Grab;
	public bool Equip;
	public bool Use;
	public string UsePromptOverride = "";
}

/// <summary>
/// This component attaches to a PlayerCharacter and handles interaction events from the PlayerCharacter that owns it
/// handles passing events to the correct components or executing correct interaction functionality for equipping, grabbing, etc
/// </summary>

public partial class InteractionComponent : ComponentBase
{
	[Signal] public delegate void InteractedEventHandler(Node3D item);
	[Signal] public delegate void GrabbedEventHandler(RigidBody3D item);
	[Signal] public delegate void DroppedEventHandler(RigidBody3D item, bool thrown);
	[Signal] public delegate void InteractionContextUpdatedEventHandler(InteractionContext newContext);

	private AttentionComponent _attentionComponent;

	private InteractionContext _interactionContext = new()
	{
		Grab = false,
		Equip = false,
		Use = false
	};

	public KeychainComponent Keychain { get; protected set; }

	private CharacterBase _owner;
	
	public override void _Ready()
	{
		base._Ready();
		
		if (!Active || Engine.IsEditorHint())
		{
			return;
		}
		
		_owner = GetOwner<CharacterBase>();

		if (_owner == null)
		{
			return;
		}

		_owner.CharacterAttention.FocusChanged += _UpdateInteractionContext;
		Keychain = new KeychainComponent();
		AddChild(Keychain);
	}

	private void _UpdateInteractionContext(Node3D newFocus, Node3D previousFocus)
	{
		if (newFocus == null)
		{
			_interactionContext.Grab = false;
			_interactionContext.Equip = false;
			_interactionContext.Use = false;
		}
		else
		{
			if (newFocus.GetNodeOrNull("Usable") is UsableComponent u)
			{
				_interactionContext.Use = true;
				_interactionContext.UsePromptOverride = u.PromptOverride;
			}
			
			// override prompt override if interactable is locked
			if (newFocus.GetNodeOrNull("Lockable") is LockableComponent { Locked: true } l)
			{
				_interactionContext.UsePromptOverride = Keychain.HasKey(l.KeyChannel) ? "LMB to Unlock" : "Locked";
			}
			
			_interactionContext.Grab = newFocus is RigidBody3D;
			_interactionContext.Equip = _owner.CharacterEquipment.CanEquip(newFocus) != null;
		}
		
		GD.Print(newFocus?.Name);
		GD.Print(_interactionContext.Grab);
		GD.Print(_interactionContext.Equip);
		GD.Print(_interactionContext.Use);
		EmitSignal(SignalName.InteractionContextUpdated, _interactionContext);
	}

	public bool CanInteract()
	{
		return _interactionContext != null;
	}

	public override void _ExitTree()
	{
		_interactionContext.Free();
		base._ExitTree();
	}

	public bool Interact(bool isAlt = false)
	{
		if (!CanInteract())
		{
			return false;
		}
  //
		Node3D target = (Node3D)_owner.CharacterAttention.CurrentFocus;
  //
		if (target == null)
		{
			return false; 
		}

		var usableNode = target.GetNodeOrNull<UsableComponent>("Usable");

		if (usableNode == null)
		{
			return false;
		}

		var lockableNode = target.GetNodeOrNull<LockableComponent>("Lockable");

		if (lockableNode is not { Locked: true })
		{
			return usableNode.Interact(_owner);
		}
		
		return lockableNode.Unlock(Keychain.Keys);
	}

	private void CheckInteraction(Node3D targetItem)
	{

	}

	private void Use(Node3D targetItem)
	{

	}

	private void Equip()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}

	private void Take()
	{
		System.Diagnostics.Debug.Assert(false, "Not yet implemented!");
	}
}
