#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

public static class SerializedPropertyExtensions
{
  /// <summary>
  /// Gets all children of `SerializedProperty` at 1 level depth.
  /// </summary>
  /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
  /// <returns>Collection of `SerializedProperty` children.</returns>
  public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
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
  /// Gets visible children of `SerializedProperty` at 1 level depth.
  /// </summary>
  /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
  /// <returns>Collection of `SerializedProperty` children.</returns>
  public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
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
}
#endif