using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

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
      var modifier = gameObject.GetComponent<NavMeshModifier>();
      if (modifier!=null) return;		// Already set
      modifier = gameObject.AddComponent<NavMeshModifier>();
      modifier.overrideArea = true;
      modifier.area = NavMesh.GetAreaFromName(NonWalkableNavMeshAreaName);
    }

    public static bool IsNotWalkable(GameObject gameObject)
    {
      var modifier = gameObject.GetComponent<NavMeshModifier>();
      return modifier==null || !modifier.overrideArea || modifier.area == NavMesh.GetAreaFromName(NonWalkableNavMeshAreaName);
    }

    /// Creates a NavMeshModifier on \c dest, cloning info from \c src.
    public static void Clone_Modifier(GameObject dest, GameObject src)
    {
      var srcComponent = src.GetComponent<NavMeshModifier>();
      if (srcComponent == null || !srcComponent.enabled) return;
      var destComponent = dest.AddComponent<NavMeshModifier>();

      destComponent.ignoreFromBuild = srcComponent.ignoreFromBuild;
      destComponent.overrideArea = srcComponent.overrideArea;
      destComponent.area = srcComponent.area;
      CopyPrivateMember<NavMeshModifier, List<int>>(destComponent, srcComponent, "m_AffectedAgents", v => new List<int>(v));
    }

    /// Copies a private member, as if we were writing <code>dest.member = src.member</code>.
    static void CopyPrivateMember<ObjectType>(object dest, object src, string memberName) { CopyPrivateMember<ObjectType, object>(dest, src, memberName, v=>v); }

    /// Copies a private member, as if we were writing <code>dest.member = copier(src.member)</code>.
    static void CopyPrivateMember<ObjectType, T>(object dest, object src, string memberName, System.Func<T, T> copier)
    {
      var field = typeof(ObjectType).GetField(memberName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      if (field==null) {
        Debug.LogErrorFormat("Could not find private field '{0}'.", memberName);
        return;
      }
      var srcField = field.GetValue(src);
      field.SetValue(dest, copier((T)srcField));
    }
  }
}