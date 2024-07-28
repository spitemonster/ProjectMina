using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class HitResult
{
	public Vector3 HitPosition { get; private set; }
	public Vector3 HitNormal { get; private set; }
	public PhysicsBody3D Collider { get; private set; }
	public CsgShape3D ColliderShape { get; private set; }
	public float Strength { get; private set; }
	
	public HitResult(Vector3 hitPosition, Vector3 hitNormal, PhysicsBody3D collider, CsgShape3D csgCollider = null, float strength = 1.0f)
	{
		HitPosition = hitPosition;
		HitNormal = hitNormal;
		Collider = collider;
		Strength = strength;
		ColliderShape = csgCollider;
	}
}

public partial class Cast : Node3D
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
	public static HitResult Ray(PhysicsDirectSpaceState3D spaceState, Vector3 castOrigin, Vector3 castEnd, Array<Rid> exclude, bool debugLine = false, bool debugHit = false, float duration = 0.0f)
	{
		PhysicsRayQueryParameters3D traceQuery = new()
		{
			From = castOrigin,
			To = castEnd,
			Exclude = exclude
		};
		
				
		if (debugLine)
		{
			DebugDraw.Line(castOrigin, castEnd, Colors.Red, duration);
		}

		var traceResult = spaceState.IntersectRay(traceQuery);

		if (traceResult == null || !traceResult.ContainsKey("collider") || (GodotObject)traceResult["collider"] is not PhysicsBody3D)
		{
			return null;
		}

		HitResult result = new((Vector3)traceResult["position"], (Vector3)traceResult["normal"],
			(PhysicsBody3D)traceResult["collider"]);

		if (debugHit)
		{
			DebugDraw.Sphere(result.HitPosition, new SphereShape3D() { Radius = .3f }, Colors.Red, duration);
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
			Radius = radius,
		};
		
		PhysicsShapeQueryParameters3D traceQuery = new()
		{
			CollideWithAreas = false,
			CollideWithBodies = true,
			Exclude = exclude,
			Shape = sphere,
			Transform = new()
			{
				Origin = origin + new Vector3(0, 1f, 0)
			}
		};
		
		DebugDraw.Sphere(traceQuery.Transform.Origin, .5f, Colors.DarkGoldenrod, 5f);

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

		var hitNormal = traceResult.ContainsKey("normal") ? (Vector3)traceResult["normal"] : Vector3.Zero;
		var hitPosition = traceResult.ContainsKey("position") ? (Vector3)traceResult["position"] : Vector3.Zero;
		// var collider = (PhysicsBody3D)traceResult["collider"];
		PhysicsBody3D collider = null;
		CsgShape3D colliderShape = null;
		
		DebugDraw.Sphere(hitPosition, 1.25f, Colors.Purple, 5f);
		
		if ((GodotObject)traceResult["collider"] is PhysicsBody3D p)
		{
			collider = p;
		}
		
		if ((GodotObject)traceResult["collider"] is CsgShape3D c)
		{
			colliderShape = c;
		}
		
		HitResult result = new(hitPosition, hitNormal, collider, colliderShape);
		
		if (debugShape)
		{
			// DebugDraw.Sphere(origin, .5f, Colors.Cyan);
		}

		if (debugHit)
		{
			DebugDraw.Sphere(result.HitPosition, .3f, Colors.Red);
		}

		return result;
	}

	public static bool TargetRay(PhysicsDirectSpaceState3D spaceState, Vector3 castOrigin, Node3D target,
		Array<Rid> exclude, bool debugLine = false, bool debugHit = false, float duration = 0.0f)
	{
		PhysicsRayQueryParameters3D traceQuery = new()
		{
			From = castOrigin,
			To = target.GlobalPosition,
			Exclude = exclude
		};
		
				
		if (debugLine)
		{
			DebugDraw.Line(castOrigin, target.GlobalPosition, Colors.Red, duration);
		}

		var traceResult = spaceState.IntersectRay(traceQuery);

		return traceResult != null && traceResult.ContainsKey("collider") && (GodotObject)traceResult["collider"] == target;
	}

	// returns a value between 0 and 1 representing the number of traces that hit the given target
	public static float TargetCone(PhysicsDirectSpaceState3D spaceState, Vector3 origin, Node3D target,
		Array<Rid> exclude, int count = 8, float halfAngleDegrees = 10f, bool debugShape = false, bool debugHit = false)
	{
		Array<int> results = new();
		var dir = origin.DirectionTo(target.GlobalPosition);
		var dist = origin.DistanceTo(target.GlobalPosition);
		for (int i = 0; i < count; i++)
		{
			
			var randDir = Utilities.Math.GetRandomConeVector(dir, halfAngleDegrees);
			HitResult castRes = Ray(spaceState, origin, randDir * dist, exclude, true, true, 10f);
			results.Add(castRes.Collider == target ? 1 : 0);
		}
		
		return (float)results.Average();
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
