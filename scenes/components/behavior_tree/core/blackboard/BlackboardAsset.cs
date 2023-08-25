using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class BlackboardAsset : Resource
{
	[Export] public Dictionary<string, Variant> Entries = new();
}
