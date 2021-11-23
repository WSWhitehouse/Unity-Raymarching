#if UNITY_EDITOR

using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/* NOTE(WSWhitehouse):
 * This script contains all unity editor functions and menu items. 
 */

namespace RaymarchEditor
{
  public static class MenuItems
  {
    [MenuItem("Raymarching/Update All Raymarch Scenes")]
    public static void UpdateAllRaymarchScenes()
    {
      if (Application.isPlaying) Debug.LogError("Cannot update all raymarch scenes while playing!");

      Debug.Log("Updating all raymarch scenes");

      string[] guids = AssetDatabase.FindAssets($"t:{nameof(SceneAsset)}", null);

      List<string> scenePaths = new List<string>(guids.Length);
      scenePaths.AddRange(guids.Select(AssetDatabase.GUIDToAssetPath));

      Scene currentScene = SceneManager.GetActiveScene();
      string currentScenePath = string.Empty;


      foreach (var path in scenePaths)
      {
        // NOTE(WSWhitehouse): Ignore scenes that are located in the packages
        if (path.StartsWith("Packages/")) continue;

        Scene openScene = EditorSceneManager.OpenScene(path);

        if (openScene.name == currentScene.name)
        {
          currentScenePath = path;
        }

        if (RaymarchScene.Get() == null) continue;
        RaymarchShaderGen.GenerateRaymarchShader();
      }

      if (!string.IsNullOrEmpty(currentScenePath))
        EditorSceneManager.OpenScene(currentScenePath);
    }

    [MenuItem("Raymarching/Generate Raymarch Shader %#x")] // shortcut: Ctrl + Shift + X
    public static void GenerateRaymarchShader()
    {
      RaymarchShaderGen.GenerateRaymarchShader();
    }

    [MenuItem("Raymarching/Force Render Scene %#z")] // shortcut: Ctrl + Shift + Z
    public static void ForceRenderScene()
    {
      RaymarchShaderGen.ForceRenderScene();
    }

    [MenuItem("GameObject/Raymarching/Create Scene")]
    public static void CreateRaymarchScene()
    {
      var rmScene = RaymarchScene.Get();
      if (rmScene != null)
      {
        Debug.LogError($"There is already an RM Scene in this scene ({SceneManager.GetActiveScene().name})");
        EditorGUIUtility.PingObject(rmScene);
        Selection.SetActiveObjectWithContext(rmScene.gameObject, rmScene.gameObject);
        return;
      }

      var obj = new GameObject("RM Scene");
      rmScene = obj.AddComponent<RaymarchScene>();
      
      Selection.SetActiveObjectWithContext(obj, obj);
    }

    [MenuItem("GameObject/Raymarching/Create Object")]
    public static void CreateRaymarchObject()
    {
      var obj = new GameObject("RM Object");
      obj.AddComponent<RaymarchObject>();
      
      Selection.SetActiveObjectWithContext(obj, obj);
    }

    [MenuItem("GameObject/Raymarching/Create Operation")]
    public static void CreateRaymarchOperation()
    {
      var obj = new GameObject("RM Operation");
      obj.AddComponent<RaymarchOperation>();
      
      Selection.SetActiveObjectWithContext(obj, obj);
    }
  }
}
#endif