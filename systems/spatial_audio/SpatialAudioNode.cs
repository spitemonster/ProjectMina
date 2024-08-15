using Godot;
using System;

public partial class SpatialAudioNode : Node3D
{
	public int Index { get; private set; } = -1;

	public bool SetIndex(uint newIndex)
	{
		if (Index > -1)
		{
			return false;
		}

		Index = (int)newIndex;
		return true;
	}
}
