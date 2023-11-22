using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace ProjectMina;
public partial class Cast : Node
{
	/// <summary>
	/// helper for performing a raycast
	/// </summary>
	/// <param name="spaceState">the physics space state from the 3d world in which we'd like to trace</param>
	/// <param name="castOrigin">origin of cast</param>
	/// <param name="castEnd">end position of cast</param>
	/// <param name="exclude">rids of nodes to exclude</param>
	/// <param name="debugLine"></param>
	/// <param name="debugHit"></param>
	/// <returns></returns>
	public static HitResult Ray(PhysicsDirectSpaceState3D spaceState, Vector3 castOrigin, Vector3 castEnd, Array<Rid> exclude, bool debugLine = false, bool debugHit = false)
	{

		PhysicsRayQueryParameters3D traceQuery = new()
		{
			From = castOrigin,
			To = castEnd,
			Exclude = exclude
		};

		var traceResult = spaceState.IntersectRay(traceQuery);

		if (traceResult == null || !traceResult.ContainsKey("collider"))
		{
			return null;
		}

		HitResult result = new()
		{
			Collider = (PhysicsBody3D)traceResult["collider"],
			HitNormal = (Vector3)traceResult["normal"],
			HitPosition = (Vector3)traceResult["position"]
		};

		if (debugLine)
		{
			DebugDraw.Line(castOrigin, castEnd, Colors.Red);
		}

		if (debugHit)
		{
			DebugDraw.Sphere(result.HitPosition, new SphereShape3D() { Radius = .3f }, Colors.Red);
		}

		return result;
	}
}
