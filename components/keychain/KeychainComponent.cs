using Godot;

using Godot.Collections;

namespace ProjectMina;

public partial class KeychainComponent : ComponentBase
{
	public Array<EKeyChannel> Keys = new();

	public bool HasKey(EKeyChannel keyChannel)
	{
		return Keys.Contains(keyChannel);
	}

	public bool AddKey(EKeyChannel keyChannel)
	{
		if (Keys.Contains(keyChannel))
		{
			return false;
		}
		
		Keys.Add(keyChannel);
		return true;
	}
}
