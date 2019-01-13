namespace NavMeshSurface2DBaker
{
  /// <summary>
  /// Data structure representing a half-edge (https://en.wikipedia.org/wiki/Doubly_connected_edge_list)
  /// This structure assumes we have a vertex class with a reference to a half edge going from that vertex
  /// and a face (triangle) class with a reference to a half edge which is a part of this face 
  /// </summary>
  public class HalfEdge
  {
    /// <summary>
    /// The face this edge is a part of.
    /// </summary>
    public Triangle Triangle { get; set; }

    /// <summary>
    /// The next edge.
    /// </summary>
    public HalfEdge NextEdge { get; set; }

    /// <summary>
    /// The previous edge.
    /// </summary>
    public HalfEdge PreviousEdge { get; set; }

    /// <summary>
    /// The edge going in the opposite direction.
    /// </summary>
    public HalfEdge OppositeEdge { get; set; }

    //The vertex the edge points to
    private Vertex _vertex;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vertex">The vertex the edge points to.</param>
    public HalfEdge(Vertex vertex)
    {
      this._vertex = vertex;
    }
  }
}