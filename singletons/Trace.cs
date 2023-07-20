using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class Trace : Node
{
	public static HitResult Line(PhysicsDirectSpaceState3D spaceState, Vector3 traceOrigin, Vector3 traceEnd, Godot.Collections.Array<Godot.Rid> exclude, bool debugLine = false, bool debugHit = false)
	{

		PhysicsRayQueryParameters3D traceQuery = new()
		{
			From = traceOrigin,
			To = traceEnd,
			Exclude = exclude
		};

		Dictionary traceResult = spaceState.IntersectRay(traceQuery);

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

		return result;
	}
}
