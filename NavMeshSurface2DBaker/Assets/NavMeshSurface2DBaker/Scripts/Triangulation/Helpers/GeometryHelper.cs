using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavMeshSurface2DBaker
{
  public static class GeometryHelper
  {
    //Is a triangle in 2d space oriented clockwise or counter-clockwise
    //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
    //https://en.wikipedia.org/wiki/Curve_orientation
    /// <summary>
    /// Returns, if a triangle made up of 3 points is oriented clockwise or counterclockwise. Obviously, order of points does matter.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>6
    /// <param name="p3"></param>
    /// <returns></returns>
    public static bool TriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
      var isClockWise = true;

      var determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

      if (determinant > 0f)
      {
        isClockWise = false;
      }

      return isClockWise;
    }

    /// <summary>
    /// Determines if a given point is inside the area of a triangle.
    /// </summary>
    /// <param name="triangleCorner1"></param>
    /// <param name="triangleCorner2"></param>
    /// <param name="triangleCorner3"></param>
    /// <param name="pointToTest"></param>
    /// <returns>True if point is within triangle. On the border does NOT count as inside.</returns>
    public static bool PointInsideTriangleArea(Vector2 triangleCorner1, Vector2 triangleCorner2, Vector2 triangleCorner3, Vector2 pointToTest)
    {
      var insideTriangle = false;

      //Based on Barycentric coordinates
      var denominator = ((triangleCorner2.y - triangleCorner3.y) * (triangleCorner1.x - triangleCorner3.x) + (triangleCorner3.x - triangleCorner2.x) * (triangleCorner1.y - triangleCorner3.y));

      var a = ((triangleCorner2.y - triangleCorner3.y) * (pointToTest.x - triangleCorner3.x) + (triangleCorner3.x - triangleCorner2.x) * (pointToTest.y - triangleCorner3.y)) / denominator;
      var b = ((triangleCorner3.y - triangleCorner1.y) * (pointToTest.x - triangleCorner3.x) + (triangleCorner1.x - triangleCorner3.x) * (pointToTest.y - triangleCorner3.y)) / denominator;
      var c = 1 - a - b;

      //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
      //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
      //{
      //    insideTriangle = true;
      //}

      //The point is within the triangle
      if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
      {
        insideTriangle = true;
      }

      return insideTriangle;
    }

    /// <summary>
    /// Determines if a non complex polygon is oriented clockwise (CW) or counter-clockwise (CCW).
    /// Works for convex as well as concave polygons.
    /// </summary>
    /// <param name="vertices">Ordered vertices defining th polygon</param>
    /// <returns>Polygon oriented CW (true) or CCW (false)</returns>
    /// /// <exception cref="ArgumentException">If fewer than 3 vertices are provided.</exception>
    public static bool PolygonOrientedClockwise(List<Vertex> vertices)
    {
      if (vertices.Count < 3)
      {
        throw new ArgumentException($"A polygon needs at least 3 vertices. Vertices provided: {vertices.Count}");
      }

      //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order

      var sum = 0f;
      var vertexCount = vertices.Count; //only get count once
      for (var i = 0; i < vertexCount; i++)
      {
        var indexOfNextVertex = CollectionsHelper.WrapIndex(i + 1, vertexCount);

        var x1 = vertices[i].Position.x;
        var y1 = vertices[i].Position.y;
        var x2 = vertices[indexOfNextVertex].Position.x;
        var y2 = vertices[indexOfNextVertex].Position.y;

        sum += (x2 - x1) * (y2 + y1);
      }

      // sum > 0 -> clockwise
      // sum = 0 -> Positive and negative areas cancel out, as in a figure-eight, probably not intended if that ever happens. No idea if this would still work; log warning
      // sum < 0 -> counter-clockwise
      if (Math.Abs(sum) < 0.000010f)
      {
        Debug.LogWarning("Sum of all positive and negative areas of polygon cancel out, something's probably wrong with your polygon.");
      }

      return sum > 0;
    }
  }
}
