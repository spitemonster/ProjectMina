using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class HitResult
{
	public Vector3 HitPosition { get; private set; }
	public Vector3 HitNormal { get; private set; }
	public PhysicsBody3D Collider { get; private set; }
	
	public HitResult(Vector3 hitPosition, Vector3 hitNormal, PhysicsBody3D collider)
	{
		HitPosition = hitPosition;
		HitNormal = hitNormal;
		Collider = collider;
	}
}

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

		HitResult result = new((Vector3)traceResult["position"], (Vector3)traceResult["normal"],
			(PhysicsBody3D)traceResult["collider"]);

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

	// casts a single shape of a given radius at the given origin
	// relatively fast so could be used 
	public static HitResult Sphere(PhysicsDirectSpaceState3D spaceState, Vector3 origin, float radius,
		Array<Rid> exclude, bool debugShape = false, bool debugHit = false)
	{

		SphereShape3D sphere = new SphereShape3D()
		{
			Radius = radius
		};
		
		PhysicsShapeQueryParameters3D traceQuery = new()
		{
			CollideWithAreas = true,
			CollideWithBodies = true,
			Exclude = exclude,
			Shape = sphere
		};

		var traceResults = spaceState.IntersectShape(traceQuery, 1);
		
		if (traceResults == null || traceResults.Count < 1)
		{
			return null;
		}

		var traceResult = traceResults[0];

		if (traceResult == null || !traceResult.ContainsKey("collider"))
		{
			return null;
		}
		
		HitResult result = new((Vector3)traceResult["position"], (Vector3)traceResult["normal"],
			(PhysicsBody3D)traceResult["collider"]);
		
		if (debugShape)
		{
			DebugDraw.Sphere(origin, .5f, Colors.Cyan);
		}

		if (debugHit)
		{
			DebugDraw.Sphere(result.HitPosition, .3f, Colors.Red);
		}

		return result;
	}

	// casts a single shape of a given radius at the given origin
	public static HitResult Shape(PhysicsDirectSpaceState3D spaceState, Shape3D shape, Vector3 origin, Array<Rid> exclude, bool debugShape = false, bool debugHit = false)
	{
		PhysicsShapeQueryParameters3D traceQuery = new()
		{
			CollideWithAreas = true,
			CollideWithBodies = true,
			Exclude = exclude,
			Shape = shape
		};

		var traceResults = spaceState.IntersectShape(traceQuery, 1);
		
		if (traceResults == null || traceResults.Count < 1)
		{
			return null;
		}

		var traceResult = traceResults[0];

		if (!traceResult.ContainsKey("collider"))
		{
			return null;
		}
		
		HitResult result = new((Vector3)traceResult["position"], (Vector3)traceResult["normal"],
			(PhysicsBody3D)traceResult["collider"]);
		
		if (debugShape)
		{
			DebugDraw.Sphere(origin, .5f, Colors.Cyan);
		}

		if (debugHit)
		{
			DebugDraw.Sphere(result.HitPosition, .3f, Colors.Red);
		}

		return result;
	}
}
