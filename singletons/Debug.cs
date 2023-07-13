using System;
using Godot;

namespace ProjectMina;

internal partial class Debug : MeshInstance3D
{
	internal static void Assert(bool condition, string msg)
	{
		if (condition)
		{
			return;
		}
		throw new ApplicationException($"Assert Failed: {msg}");
	}

	internal static void DrawSphere(Vector3 center, float radius, Color color = new())
	{
		int step = 4;
		float stpi = 2 * Mathf.Pi / step;
		Godot.Collections.Array<Godot.Collections.Array<Vector3>> axes = new(){
			new() {
				Vector3.Up,
				Vector3.Right
			},
			new() {
				Vector3.Right,
				Vector3.Forward
			},
			new() {
				Vector3.Forward,
				Vector3.Up
			}
		};

		// Mesh.Surface
	}
}