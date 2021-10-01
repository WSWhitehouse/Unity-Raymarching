using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchScene : MonoBehaviour
{
#if UNITY_EDITOR
  private void OnEnable()
  {
    if (Application.isPlaying) return;
    EditorApplication.hierarchyChanged += BuildTree;
  }

  private void OnDisable()
  {
    if (Application.isPlaying) return;
    EditorApplication.hierarchyChanged -= BuildTree;
  }

  private void OnValidate()
  {
    BuildTree();
  }
#endif
  
  private void Start()
  {
    BuildTree();
  }
  
  private void OnDestroy()
  {
    Raymarch.SetObjects(new List<RaymarchObject>());
    Raymarch.SetModifiers(new Dictionary<int, RaymarchModifier>());
  }

  private List<RaymarchObject> _objects = new List<RaymarchObject>();
  private Dictionary<int, RaymarchModifier> _modifiers = new Dictionary<int, RaymarchModifier>();

  private void BuildTree()
  {
    _objects.Clear();
    _modifiers.Clear();
    
    Scene activeScene = SceneManager.GetActiveScene();

    if (!activeScene.isLoaded || !activeScene.IsValid())
    {
      Raymarch.SetObjects(new List<RaymarchObject>());
      Raymarch.SetModifiers(new Dictionary<int, RaymarchModifier>());
      return;
    }

    List<GameObject> rootGameObjects = new List<GameObject>(activeScene.rootCount);
    activeScene.GetRootGameObjects(rootGameObjects);

    foreach (var rootGameObject in rootGameObjects)
    {
      AddObjToTree(rootGameObject);
    }

    Raymarch.SetObjects(_objects);
    Raymarch.SetModifiers(_modifiers);
  }

  private void AddObjToTree(GameObject gameObject)
  {
    var rmObject = gameObject.GetComponent<RaymarchObject>();
    if (rmObject != null)
    {
      _objects.Add(rmObject);
    }

    var rmMod = gameObject.GetComponent<RaymarchModifier>();
    int index = _objects.Count;

    foreach (Transform child in gameObject.transform)
    {
      AddObjToTree(child.gameObject);
    }

    if (rmMod != null)
    {
      rmMod.NumOfObjects = _objects.Count - index;
      rmMod.Index = _modifiers.Count;

      _modifiers.Add(index, rmMod);
    }
  }
}