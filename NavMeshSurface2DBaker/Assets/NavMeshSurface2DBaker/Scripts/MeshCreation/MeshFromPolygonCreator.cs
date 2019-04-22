using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NavMeshSurface2DBaker
{
  public static class MeshFromPolygonCreator
  {
    private const float MeshDepth = 1;

    /// <summary>
    /// Old debug function.
    /// Searches the object this script is attached to for a <see cref="PolygonCollider2D"/>, takes the first it finds and creates a mesh from the collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="parentToAttachTemporaryObjectsTo"></param>
    [Obsolete("Old debug function")]
    public static void CreateMeshFromPolygonCollider2D(PolygonCollider2D collider, Transform parentToAttachTemporaryObjectsTo)
    {
      CreateMesh(collider.points, collider.transform, parentToAttachTemporaryObjectsTo);
    }

    /// <summary>
    /// Creates a mesh from the points defining a polygon.
    /// </summary>
    /// <param name="polygonPoints">Points defining the polygon, need to be ordered but can be ordered cw or ccw.</param>
    /// <param name="transformPolygonColliderBelongsTo">Transform of polygon collider the points belong to. Points need to be from polygon in x/y space!</param>
    /// <param name="parentToAttachTemporaryObjectsTo">Parent to attach the created mesh to.</param>
    /// <returns></returns>
    public static GameObject CreateMesh(Vector2[] polygonPoints, Transform transformPolygonColliderBelongsTo, Transform parentToAttachTemporaryObjectsTo)
    {
      var polygonTransformPosition = transformPolygonColliderBelongsTo.position;

      //create mesh
      var mesh = CreateBaseMesh(polygonPoints);
      mesh = CreateExtrudedMeshFromBaseMesh(mesh);

      //create gameobject and attach mesh
      var go = new GameObject("Mesh");
      
      //mesh now directly lies on plane PolygonCollider2D was created on and extrudes into positive z direction.
      //Take it back a little so it 100% penetrates plane and we won't have an edge case where it might not get detected by the navmesh builder.
      // ReSharper disable once PossibleLossOfFraction
      go.transform.position = new Vector3(polygonTransformPosition.x, polygonTransformPosition.y, -MeshDepth);
      var meshFilter = go.AddComponent<MeshFilter>();
      meshFilter.mesh = mesh;
      go.AddComponent<MeshRenderer>();

      //apply rotation and scale of original collider transform
      go.transform.localScale = transformPolygonColliderBelongsTo.lossyScale;
      go.transform.rotation = transformPolygonColliderBelongsTo.rotation;

      //attach transform to parent. Doing this at the end because it's e.g. easier to set scale when transform has no parent
      go.transform.parent = parentToAttachTemporaryObjectsTo;

      return go;
    }

    /// <summary>
    /// Creates a flat mesh from the points defining a polygon.
    /// </summary>
    /// <param name="polygonPoints"></param>
    /// <returns></returns>
    private static Mesh CreateBaseMesh(Vector2[] polygonPoints)
    {
      var polygonPointsAsVector3 = polygonPoints.Select(point => (Vector3) point).ToList();

      //calculate mesh triangles
      var vertexIndexLookup = new Dictionary<Vertex, int>();
      var vertexList = new List<Vertex>();
      for (var i = 0; i < polygonPointsAsVector3.Count; i++)
      {
        var vertex = new Vertex(polygonPointsAsVector3[i]);
        vertexList.Add(vertex);
        vertexIndexLookup.Add(vertex, i);
      }

      //triangulate
      var triangles = Triangulator.TriangulateConcaveOrConvexPolygon(vertexList);

      var triangleVertexIndicesList = new List<int>();
      foreach (var triangle in triangles)
      {
        triangleVertexIndicesList.Add(vertexIndexLookup[triangle.Vertex1]);
        triangleVertexIndicesList.Add(vertexIndexLookup[triangle.Vertex2]);
        triangleVertexIndicesList.Add(vertexIndexLookup[triangle.Vertex3]);
      }

      //create mesh
      var mesh = new Mesh
      {
        vertices = polygonPointsAsVector3.ToArray(),
        triangles = triangleVertexIndicesList.ToArray()
      };

      return mesh;
    }

    /// <summary>
    /// Takes a flat mesh and extrudes the mesh to make it 3 dimensional.
    /// </summary>
    /// <param name="baseMesh">Flat mesh to extrude. Flat mesh needs to be in x/y space!</param>
    /// <returns></returns>
    private static Mesh CreateExtrudedMeshFromBaseMesh(Mesh baseMesh)
    {
      //new mesh will have more vertices and triangles than base mesh, make new lists for them and copy data from base mesh.
      //since these are base types and structs, values will be copied and operating on base mesh won't have an influence on the lists.
      var vertices = new List<Vector3>(baseMesh.vertices);
      var triangles = new List<int>(baseMesh.triangles);

      //base mesh shows the "front" and we'll extrude into the screen / positive z
      //add "back" mesh which is same as front but further in and all triangle normals are inverted ( = face flipped)
      var baseMeshVertices = baseMesh.vertices;
      for (var i = 0; i < baseMeshVertices.Length; i++)
      {
        vertices.Add(new Vector3(baseMeshVertices[i].x, baseMeshVertices[i].y, MeshDepth));
      }

      var baseMeshTriangles = baseMesh.triangles;
      for (var i = 0; i < baseMeshTriangles.Length; i++)
      {
        //add counterclockwise because surface shall face the back
        var reminder = i % 3;
        switch (reminder)
        {
          case 0:
            triangles.Add(baseMeshTriangles[i] + baseMeshVertices.Length);
            break;
          case 1:
            triangles.Add(baseMeshTriangles[i + 1] + baseMeshVertices.Length);
            break;
          case 2:
            triangles.Add(baseMeshTriangles[i - 1] + baseMeshVertices.Length);
            break;
        }
      }

      //now we have the front and back, we need to add the sides. to do this, we want to connect all boundary edges of the polygon, which requires 2 steps

      //step 1 make list of all edges and filter out all internal edges so we only have the bounding edges, describing the shape of the polygon.
      //internal edges can be filtered by removing all edges that exist twice but with inverted direction (e.g. edge from vertex 1 to 5 and one from 5 to 1)
      var edges = new List<Tuple<int, int>>(); //integers will represent vertex indices
      var edgesToRemove = new List<Tuple<int, int>>();

      for (var i = 0; i < baseMeshTriangles.Length; i++)
      {
        var reminder = i % 3;

        var indexVertex1 = baseMeshTriangles[i];
        var indexVertex2 = reminder == 2 ? baseMeshTriangles[i - 2] : baseMeshTriangles[i + 1];

        var inverseEdge = edges.FirstOrDefault(e => e.Item1 == indexVertex2 && e.Item2 == indexVertex1);
        if (inverseEdge != null)
        {
          //inverse edge already exist in edges -> don't add this edge and mark inverse edge for deletion
          edgesToRemove.Add(inverseEdge);
        }
        else
        {
          edges.Add(new Tuple<int, int>(indexVertex1, indexVertex2));
        }
      }

      //remove remaining internal edges
      foreach (var edge in edgesToRemove)
      {
        edges.Remove(edge);
      }

      //step 2 for each edge, add two triangles that will make up the side connecting the front and back of the mesh
      foreach (var (vertex1, vertex2) in edges)
      {
        triangles.Add(vertex1);
        triangles.Add(vertex1 + baseMeshVertices.Length);
        triangles.Add(vertex2);
        triangles.Add(vertex2);
        triangles.Add(vertex1 + baseMeshVertices.Length);
        triangles.Add(vertex2 + baseMeshVertices.Length);
      }

      //create mesh
      var mesh = new Mesh
      {
        name = "TempMesh",
        vertices = vertices.ToArray(),
        triangles = triangles.ToArray()
      };

      return mesh;
    }
  }
}
