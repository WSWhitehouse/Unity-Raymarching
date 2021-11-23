using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Util
{
#if UNITY_EDITOR
  public static class Editor
  {
    /// <summary>
    /// Gets all children of <see cref="SerializedProperty"/> at 1 level depth
    /// </summary>
    /// <param name="serializedProperty"><see cref="SerializedProperty"/> to get children</param>
    /// <returns>Collection of <see cref="SerializedProperty"/> children</returns>
    public static IEnumerable<SerializedProperty> GetSerializedPropertyChildren(SerializedProperty serializedProperty)
    {
      SerializedProperty currentProperty = serializedProperty.Copy();
      SerializedProperty nextSiblingProperty = serializedProperty.Copy();
      {
        nextSiblingProperty.Next(false);
      }

      if (!currentProperty.Next(true)) yield break;

      do
      {
        if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty)) break;

        yield return currentProperty.Copy();
      } while (currentProperty.Next(false));
    }

    /// <summary>
    /// Gets visible children of <see cref="SerializedProperty"/> at 1 level depth
    /// </summary>
    /// <param name="serializedProperty"><see cref="SerializedProperty"/> to get children</param>
    /// <returns>Collection of <see cref="SerializedProperty"/> children</returns>
    public static IEnumerable<SerializedProperty> GetSerializedPropertyVisibleChildren(
      SerializedProperty serializedProperty)
    {
      SerializedProperty currentProperty = serializedProperty.Copy();
      SerializedProperty nextSiblingProperty = serializedProperty.Copy();
      {
        nextSiblingProperty.NextVisible(false);
      }

      if (!currentProperty.NextVisible(true)) yield break;

      do
      {
        if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty)) break;

        yield return currentProperty.Copy();
      } while (currentProperty.NextVisible(false));
    }

    /// <summary>
    /// Finds all assets of type in the <see cref="AssetDatabase"/>
    /// </summary>
    /// <typeparam name="T">Type of asset to find. Must be of type <see cref="UnityEngine.Object"/></typeparam>
    /// <returns>List of all assets of type <see cref="T"/></returns>
    public static List<T> FindAllAssetsOfType<T>() where T : Object
    {
      string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);

      List<T> assets = new List<T>(guids.Length);
      assets.AddRange(guids.Select(guid => 
        AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))));

      return assets;
    }
  }
#endif

  public static class Runtime
  {
    
  }
}