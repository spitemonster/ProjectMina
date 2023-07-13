using Godot;
using Godot.Collections;

namespace ProjectMina;

// will come back to this
public class HitResult
{
	public PhysicsBody3D Collider;
	public Vector3 HitNormal;
	public Vector3 HitPosition;
}

public partial class Trace : Node
{
	// space_state: PhysicsDirectSpaceState3D, trace_origin: Vector3, trace_end: Vector3, exclude: Array[Variant], debug_line: bool = false, debug_hit: bool = false
	// public static Dictionary Line(PhysicsDirectSpaceState3D spaceState, Vector3 traceOrigin, Vector3 traceEnd, Godot.Collections.Array<Godot.Rid> exclude, bool debugLine = false, bool debugHit = false)
	// {

	// 	PhysicsRayQueryParameters3D traceQuery = new()
	// 	{
	// 		From = traceOrigin,
	// 		To = traceEnd,
	// 		Exclude = exclude
	// 	};

	// 	var traceResult = spaceState.IntersectRay(traceQuery);
	// 	return traceResult;
	// }

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
