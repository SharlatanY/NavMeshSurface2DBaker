using UnityEngine;
using UnityEngine.AI;

namespace NavMeshSurface2DBaker
{
  public static class NavigationHelper
  {
    private const string NonWalkableNavMeshAreaName = "Not Walkable";
    /// <summary>
    /// Adds a "Not Walkable" <see cref="NavMeshModifier"/> to a transform.
    /// </summary>
    /// <param name="gameObject"></param>
    public static void SetNonWalkable(GameObject gameObject)
    {
      var modifier = gameObject.AddComponent<NavMeshModifier>();
      modifier.overrideArea = true;
      modifier.area = NavMesh.GetAreaFromName(NonWalkableNavMeshAreaName);
    }
  }
}