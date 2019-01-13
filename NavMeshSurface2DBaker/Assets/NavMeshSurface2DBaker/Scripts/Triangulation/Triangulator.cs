using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NavMeshSurface2DBaker
{
  public static class Triangulator
  {
    /// <summary>
    /// Triangulates a convex polygon. Produces invalid results with concave polygons but is faster than <see cref="TriangulateConcaveOrConvexPolygon"/> when used with convex polygons.
    /// </summary>
    /// <param name="vertices">Vertices making up the polygon.</param>
    /// <returns></returns>
    public static List<Triangle> TriangulateConvexPolygon(List<Vertex> vertices)
    {
      var triangles = new List<Triangle>();

      for (var i = 2; i < vertices.Count; i++)
      {
        var vert1 = vertices[0];
        var vert2 = vertices[i];
        var vert3 = vertices[i - 1];

        triangles.Add(new Triangle(vert1, vert2, vert3));
      }

      return triangles;
    }

    /// <summary>
    /// Triangulates a convex polygon. Produces invalid results with concave polygons but is faster when using on convex polygons.
    /// </summary>
    /// <param name="vertices">Vertex locations making up the polygon.</param>
    /// <returns></returns>
    public static List<Triangle> TriangulateConvexPolygon(IEnumerable<Vector2> vertices)
    {
      var vertexList = vertices.Select(point => new Vertex(point)).ToList();

      return TriangulateConvexPolygon(vertexList);
    }

    /// <summary>
    /// Triangulates a convex or concave polygon.
    /// The points on the polygon should be ordered counter-clockwise.
    /// This algorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n).
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If fewer than 3 vertices are provided.</exception>
    public static List<Triangle> TriangulateConcaveOrConvexPolygon(List<Vertex> vertices)
    {
      if (vertices.Count < 3)
      {
        throw new ArgumentException($"A polygon needs at least 3 vertices. Vertices provided: {vertices.Count}");
      }

      var triangles = new List<Triangle>();

      //If we just have three points, we can just return a single triangle
      if (vertices.Count == 3)
      {
        triangles.Add(new Triangle(vertices[0], vertices[2], vertices[1])); //vertices are expected to be in ccw order but Unity draw order for triangles in meshes is cw, hence 0,2,1 and not 0,1,2

        return triangles;
      }

      //Step 1. Set the next and prev vertex for every vertex

      //Find the next and previous vertex
      for (var i = 0; i < vertices.Count; i++)
      {
        var nextPos = CollectionsHelper.WrapIndex(i + 1, vertices.Count);
        var prevPos = CollectionsHelper.WrapIndex(i - 1, vertices.Count);

        vertices[i].PreviousVertex = vertices[prevPos];
        vertices[i].NextVertex = vertices[nextPos];
      }

      
      //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
      foreach (var v in vertices)
      {
        SetIfVertexIsConcaveOrConvex(v);
      }

      //Have to find the ears after we have found if the vertex is concave or convex
      var earVertices = new List<Vertex>();

      foreach (var v in vertices)
      {
        SetIfVertexIsEar(v, vertices);
        if(v.IsEar)
          earVertices.Add(v);
      }


      //Step 3. Triangulate!
      while (true)
      {
        //This means we have just one triangle left
        if (vertices.Count == 3)
        {
          //The final triangle
          triangles.Add(new Triangle(vertices[0], vertices[0].PreviousVertex, vertices[0].NextVertex));

          break;
        }

        //Make a triangle of the first ear
        var earVertex = earVertices[0];

        var earVertexPrev = earVertex.PreviousVertex;
        var earVertexNext = earVertex.NextVertex;

        var newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

        triangles.Add(newTriangle);

        //Remove the vertex from the lists
        earVertices.Remove(earVertex);

        vertices.Remove(earVertex);

        //Update the previous vertex and next vertex so that they are now directly linked together (take current ear vertex out of doubly linked list)
        earVertexPrev.NextVertex = earVertexNext;
        earVertexNext.PreviousVertex = earVertexPrev;

        //...see if we have found a new ear by investigating the two vertices that was part of the ear and add them to the list of ears, if that's the case.
        SetIfVertexIsConcaveOrConvex(earVertexPrev);
        SetIfVertexIsConcaveOrConvex(earVertexNext);

        earVertices.Remove(earVertexPrev);
        earVertices.Remove(earVertexNext);

        SetIfVertexIsEar(earVertexPrev, vertices);
        if(earVertexPrev.IsEar)
          earVertices.Add(earVertexPrev);
        SetIfVertexIsEar(earVertexNext, vertices);
        if (earVertexNext.IsEar)
          earVertices.Add(earVertexNext);
      }

      return triangles;
    }

    /// <summary>
    /// Checks if a vertex is concave or convex and sets its IsConcave property accordingly-
    /// </summary>
    /// <param name="v"></param>
    private static void SetIfVertexIsConcaveOrConvex(Vertex v)
    {
      //This is a concave/reflex vertex if its triangle is oriented clockwise
      v.IsConcave = GeometryHelper.TriangleOrientedClockwise(v.PreviousVertex.GetPos2D(), v.GetPos2D(), v.NextVertex.GetPos2D());
    }

    //Check if a vertex is an ear
    /// <summary>
    /// Checks if a vertex is an ear and sets its IsEar property accordingly.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="vertices"></param>
    private static void SetIfVertexIsEar(Vertex v, IEnumerable<Vertex> vertices)
    {
      v.IsEar = false;

      //A concave/reflex vertex can't be an ear!
      if (v.IsConcave)
      {
        return;
      }

      // vertex positions that make up the triangle this vertex belongs to.
      var v1 = v.PreviousVertex.GetPos2D();
      var v2 = v.GetPos2D();
      var v3 = v.NextVertex.GetPos2D();

      //if none of the concave vertices is inside the area of the triangle this vertex belongs to, this vertex is an ear.
      var anyConcaveVertexInsideTriangleArea = (from vertex in vertices where vertex.IsConcave select vertex.GetPos2D()).Any(point => GeometryHelper.PointInsideTriangleArea(v1, v2, v3, point));

      if (!anyConcaveVertexInsideTriangleArea)
      {
        v.IsEar = true;
      }
    }
  }
}