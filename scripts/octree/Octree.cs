using Godot;
using System;
using System.Collections.Generic;

namespace ProjectMina;

public class Octree
{

    public OctreeNode RootNode;

    public Octree(Vector3 position, float worldSize, float minNodeSize)
    {
        var size = new Vector3(worldSize, worldSize, worldSize);
        var rootBounds = new Aabb(position - size / 2, size);
        
        RootNode = new OctreeNode(rootBounds, minNodeSize);
    }

    public void Add(Vector3 position)
    {
        RootNode.Add(position);
    }

    public List<Vector3> QueryPosition(Vector3 position, float radius)
    {
        return RootNode.FindPositionsInRadius(position, radius);
    }
//     private class OctreeNode
//     {
//         public Vector3 Center;
//         public float Size;
//         public List<T> Objects;
//         public OctreeNode[] Children;
//
//         public OctreeNode(Vector3 center, float size)
//         {
//             Center = center;
//             Size = size;
//             Objects = new List<T>();
//             Children = null;
//         }
//
//         public bool IsLeaf => Children == null;
//
//         public void Subdivide()
//         {
//             Children = new OctreeNode[8];
//             float halfSize = Size / 2f;
//             float quarterSize = Size / 4f;
//
//             for (int i = 0; i < 8; i++)
//             {
//                 Vector3 offset = new Vector3(
//                     ((i & 1) == 0) ? -quarterSize : quarterSize,
//                     ((i & 2) == 0) ? -quarterSize : quarterSize,
//                     ((i & 4) == 0) ? -quarterSize : quarterSize
//                 );
//                 Children[i] = new OctreeNode(Center + offset, halfSize);
//             }
//         }
//     }
//
//     private OctreeNode root;
//     private float minSize;
//
//     public Octree(Vector3 center, float size, float minSize)
//     {
//         root = new OctreeNode(center, size);
//         this.minSize = minSize;
//     }
//
//     public void Insert(T obj, Vector3 position)
//     {
//         Insert(obj, position, root);
//         DebugDraw.Sphere(position, .9f, Colors.Green, 60f);
//     }
//
//     private void Insert(T obj, Vector3 position, OctreeNode node)
//     {
//         if (node.Size <= minSize)
//         {
//             node.Objects.Add(obj);
//             return;
//         }
//
//         if (node.IsLeaf)
//         {
//             node.Subdivide();
//         }
//
//         foreach (var child in node.Children)
//         {
//             if (_IsWithinBounds(position, child))
//             {
//                 Insert(obj, position, child);
//                 return;
//             }
//         }
//
//         // Fallback: add to current node if position doesn't fit exactly into any child (shouldn't happen in ideal conditions)
//         node.Objects.Add(obj);
//     }
//
//     public List<T> Query(Vector3 position, float radius)
//     {
//         return _Query(position, radius, root);
//     }
//
//     private List<T> _Query(Vector3 position, float radius, OctreeNode node)
//     {
//         List<T> result = new List<T>();
//
//         if (node.IsLeaf)
//         {
//             result.AddRange(node.Objects);
//         }
//         else
//         {
//             foreach (var child in node.Children)
//             {
//                 if (_Intersects(position, radius, child))
//                 {
//                     result.AddRange(_Query(position, radius, child));
//                 }
//             }
//         }
//
//         foreach (var res in result)
//         {
//             
//         }
//
//         return result;
//     }
//
//     private bool _IsWithinBounds(Vector3 position, OctreeNode node)
//     {
//         return Math.Abs(position.X - node.Center.X) <= node.Size / 2 &&
//                Math.Abs(position.Y - node.Center.Y) <= node.Size / 2 &&
//                Math.Abs(position.Z - node.Center.Z) <= node.Size / 2;
//     }
//
//     private static bool _Intersects(Vector3 position, float radius, OctreeNode node)
//     {
//         float distanceSquared = 0;
//
//         distanceSquared += Math.Max(0, Math.Abs(position.X - node.Center.X) - node.Size / 2);
//         distanceSquared += Math.Max(0, Math.Abs(position.Y - node.Center.Y) - node.Size / 2);
//         distanceSquared += Math.Max(0, Math.Abs(position.Z - node.Center.Z) - node.Size / 2);
//
//         return distanceSquared <= radius * radius;
//     }
}
