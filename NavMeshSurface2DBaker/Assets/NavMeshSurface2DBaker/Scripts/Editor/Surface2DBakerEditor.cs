using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AI;
using UnityEngine.AI;

namespace NavMeshSurface2DBaker.Editor
{
  [CustomEditor(typeof(Surface2DBaker))]
  public class Surface2DBakerEditor : UnityEditor.Editor
  {
    private List<GameObject> _objectsToBeDeletedAfterBaking;
    private Surface2DBaker _surface2DBaker;

    void OnEnable()
    {
      _surface2DBaker = (Surface2DBaker)target;
    }

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();

      if (GUILayout.Button("Bake 2D"))
      {
        Bake();
      }
    }

    /// <summary>
    /// Bakes the NavMesh, including 2D Colliders.
    /// Located here and not in <see cref="Surface2DBaker"/> because we need to make calls to other editor scripts, which causes build errors when
    /// done on a MonoBehavior.
    /// </summary>
    private void Bake()
    {
      _objectsToBeDeletedAfterBaking = _surface2DBaker.CreateMeshes();
      //adding navmesh modifier to ensure the area on top of the generated meshes doesn't count as walkable
      foreach (var obj in _objectsToBeDeletedAfterBaking)
      {
        NavigationHelper.SetNonWalkable(obj);
      }
      BakeSurface();

      //subscribe for editor update so we can later on delete the generated meshes.
      //for details as to why we can't do this directly, see comments in method
      //BakeSurface().
      EditorApplication.update += OnEditorUpdate;
    }

    /// <summary>
    /// Calls the baking function of the <see cref="NavMeshSurface"/> located on the same game object as this script.
    /// </summary>
    private void BakeSurface()
    {
      if (_surface2DBaker.EnableMeshRendererBeforeBaking)
      {
        var meshRenderer = _surface2DBaker.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.enabled = true;
      }

      var surface = _surface2DBaker.gameObject.GetComponent<NavMeshSurface>();
      var surfacesToBake = new UnityEngine.Object[] { surface };
      NavMeshAssetManager.instance.StartBakingSurfaces(surfacesToBake);

      //while (NavMeshAssetManager.instance.IsSurfaceBaking(surface))
      //{
      //  absolutely DON'T wait for surface baking to be finished here for the purpose of
      //  deleting the meshes.
      //  Editor works in a way which leads to circumstances, where
      //  IsSurfaceBaking will ALWAYS return "true" while we're still in the method call stack
      //  started by pressing on the "Bake 2D button" in the GUI.
      //  Cleanup takes place in callback to OnEditorUpdate.
      //}
    }

    private void OnEditorUpdate()
    {
      var surface = _surface2DBaker.GetComponent<NavMeshSurface>();
      if (!NavMeshAssetManager.instance.IsSurfaceBaking(surface))
      {
        foreach (var obj in _objectsToBeDeletedAfterBaking)
        {
          DestroyImmediate(obj);
        }
        _objectsToBeDeletedAfterBaking.Clear();

        if (_surface2DBaker.DisableMeshRendererAfterBaking)
        {
          var meshRenderer = _surface2DBaker.GetComponent<MeshRenderer>();
          meshRenderer.enabled = false;
        }

        //This method is only added as subscriber to EditorApplication.update so we can clean up generated meshes when baking has finished.
        //Now that baking has finished, unsubscribe.
        // ReSharper disable once DelegateSubtraction
        EditorApplication.update -= OnEditorUpdate;
      }
    }
  }
}
