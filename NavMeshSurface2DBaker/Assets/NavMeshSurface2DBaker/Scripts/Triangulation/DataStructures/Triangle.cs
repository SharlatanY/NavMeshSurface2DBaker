using UnityEngine;

namespace NavMeshSurface2DBaker
{
  /// <summary>
  /// Data structure representing a triangle made out of 3 <see cref="Vertex"/> objects
  /// </summary>
  public class Triangle
  {
    //Corners
    public Vertex Vertex1 { get; set; }
    public Vertex Vertex2 { get; set; }
    public Vertex Vertex3 { get; set; }

    //If we are using the half edge mesh structure, we just need one half edge
    public HalfEdge HalfEdge { get; set; }

    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
      this.Vertex1 = v1;
      this.Vertex2 = v2;
      this.Vertex3 = v3;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
      this.Vertex1 = new Vertex(v1);
      this.Vertex2 = new Vertex(v2);
      this.Vertex3 = new Vertex(v3);
    }

    public Triangle(HalfEdge halfEdge)
    {
      this.HalfEdge = halfEdge;
    }

    //Change orientation of triangle from cw -> ccw or ccw -> cw
    public void ChangeOrientation()
    {
      var temp = this.Vertex1;
      this.Vertex1 = this.Vertex2;
      this.Vertex2 = temp;
    }
  }
}