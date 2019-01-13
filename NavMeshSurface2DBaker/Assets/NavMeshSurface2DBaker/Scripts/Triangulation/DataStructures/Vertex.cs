using UnityEngine;

namespace NavMeshSurface2DBaker
{
  /// <summary>
  /// Representing a vertex of a <see cref="NavMeshSurface2DBaker.Triangle"/>
  /// </summary>
  public class Vertex
  {
    /// <summary>
    /// World space position of the vertex
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// The outgoing HalfEdge (a HalfEdge that starts at this vertex). 
    /// Doesn't matter which edge we connect to it.
    /// </summary>
    public HalfEdge OutgoingHalfEdge { get; set; }

    /// <summary>
    /// Triangle the vertex belongs to.
    /// </summary>
    public Triangle Triangle { get; set; }

    /// <summary>
    /// Previous vertex this vertex is attached to
    /// </summary>
    public Vertex PreviousVertex { get; set; }

    /// <summary>
    /// Next vertex this vertex is attached to.
    /// </summary>
    public Vertex NextVertex { get; set; }

    /// <summary>
    /// Is vertex concave (also called "reflex")?
    /// True if polygon internal angle formed by the two edges connected to the vertex is bigger than 180°.
    /// </summary>
    public bool IsConcave { get; set; }

    /// <summary>
    /// Is this vertex an ear?
    /// </summary>
    public bool IsEar { get; set; }

    public Vertex(Vector3 position)
    {
      this.Position = position;
    }

    /// <summary>
    /// Returns the position of the vertex in 2D space.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetPos2D()
    {
      return new Vector2(Position.x, Position.y);
    }
  }
}
