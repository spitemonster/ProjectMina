using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class CombinationLock : Node3D
{
	[Signal] public delegate void CombinationUnlockedEventHandler();
	// an array allows for an arbitrary number of tumblers
	[Export] public string Combination { get; private set; }
	
	[Export] public LockableComponent Lock { get; protected set; }
	[Export] public EKeyChannel KeyChannel = EKeyChannel.CombinationChannelOne;

	[Export] public Array<Tumbler> Tumblers;
	public override void _Ready()
	{
		if (Tumblers.Count != Combination.Length)
		{
			return;
		}
		
		foreach (var tumbler in Tumblers)
		{
			tumbler.TumblerTurned += _CheckCombination;
		}
	}

	// eventually I plan on generating combinations based on a hash generated when the user begins their game
	// so we'll want a way to set the combination remotely
	public bool SetCombination(string newCombination, bool force = false)
	{
		if (Combination != null && !force)
		{
			return false;
		}

		Combination = newCombination;
		return true;
	}

	// every time a tumbler is updated, check and see if the proper combination has been entered and unlock if so
	private void _CheckCombination()
	{
		var currentCombination = "";

		foreach (var tumbler in Tumblers)
		{
			currentCombination += tumbler.GetSettingAsString();
		}
		
		if (currentCombination != Combination)
		{
			if (Lock != null && !Lock.Locked)
			{
				Lock.Lock();
			}
			
			return;
		}
		
		Lock?.Unlock(KeyChannel);
		EmitSignal(SignalName.CombinationUnlocked);
	}
}
