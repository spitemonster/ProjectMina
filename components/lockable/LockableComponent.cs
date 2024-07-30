using Godot;
using System;
using Godot.Collections;

namespace ProjectMina;

public partial class LockableComponent : ComponentBase
{
	[ExportCategory("Lock")]
	[Export] public bool CanLock { get; protected set; }
	[Export] public bool Locked { get; private set; }
	[Export] public EKeyChannel KeyChannel { get; private set; }
	
	[Signal] public delegate void LockStatusUpdatedEventHandler(bool lockStatus);
	
	public bool Lock()
	{
		if (Locked)
		{
			return false;
		}

		Locked = true;
		EmitSignal(SignalName.LockStatusUpdated, Locked);
		return true;
	}

	// overrides for accepting either a single key or a whole keychain
	public bool Unlock(EKeyChannel key)
	{
		if (!Locked || key != KeyChannel)
		{
			return false;
		}

		Locked = false;
		EmitSignal(SignalName.LockStatusUpdated, Locked);
		return true;
	}

	public bool Unlock(Array<EKeyChannel> keychain)
	{
		if (!Locked || !keychain.Contains(KeyChannel))
		{
			return false;
		}

		Locked = false;
		EmitSignal(SignalName.LockStatusUpdated, Locked);
		return true;
	}
}
