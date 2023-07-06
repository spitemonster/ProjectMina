using System;
using Godot;

namespace ProjectMina;

internal partial class Debug : Node
{
	internal static void Assert(bool condition, string msg)
	{
		if (condition)
		{
			return;
		}
		throw new ApplicationException($"Assert Failed: {msg}");
	}
}