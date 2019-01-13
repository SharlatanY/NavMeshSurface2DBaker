using UnityEditor;
using UnityEngine;

namespace NavMeshSurface2DBaker.Editor
{
  [CustomEditor(typeof(Surface2DBaker))]
  public class Surface2DBakerEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      var surface2DBaker = (Surface2DBaker)target;

      if (GUILayout.Button("Bake 2D"))
      {
        surface2DBaker.Bake();
      }
    }
  }
}
