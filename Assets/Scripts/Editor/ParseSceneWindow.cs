using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Drawing;
using UnityEditor;
using UnityEngine;
using GLTF.Schema;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityGLTF.Extensions;
using Assert = UnityEngine.Assertions.Assert;
using AssertionException = UnityEngine.Assertions.AssertionException;

public class ParseSceneWindow : EditorWindow {

    public const string title = "Parse Scene";
    public const string prefabKeyword = "prefab";
    public const string staticKeyword = "static";
    public const string layerPrefix = "layer:";
    public const string withMeshColliderKeyword = "with-mesh-collider";

    private Transform blueprint;

    [MenuItem("Tools/" + title)]
    public static void OpenWindow() {
        var window = GetWindow<ParseSceneWindow>();
        window.titleContent = new GUIContent(title);
        window.Show();
    }

    private GameObject TryFindPrefab(string name) {
        var guids = AssetDatabase.FindAssets("t:Prefab " + name);
        return guids.Length switch {
            0 => null,
            1 => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0])),
            _ => throw new Exception("Multiple prefabs found: " + string.Join(", ", guids.Select(AssetDatabase.GUIDToAssetPath)))
        };
    }

    private void OnGUI() {

        blueprint = EditorGUILayout.ObjectField(blueprint, typeof(Transform), false) as Transform;
        if (GUILayout.Button("Process") && blueprint) {

            var root = new GameObject(blueprint.name).transform;

            void Process(Transform transform) {

                var meshRenderer = transform.GetComponent<MeshRenderer>();
                var meshFilter = transform.GetComponent<MeshFilter>();
                if (meshRenderer && meshFilter && meshFilter.sharedMesh) {

                    Transform instance = null;

                    var shouldBeStatic = false;
                    int? layer = null;
                    string name = null;
                    var isPrefab = false;
                    var withMeshCollider = false;

                    var meshName = meshFilter.sharedMesh.name;
                    var words = meshName.Replace("  ", " ").Trim().Split(' ');
                    
                    foreach (var word in words) {
                        switch (word) {
                            case staticKeyword:
                                shouldBeStatic = true;
                                break;
                            case prefabKeyword:
                                isPrefab = true;
                                break;
                            case withMeshColliderKeyword:
                                withMeshCollider = true;
                                break;
                            default: {
                                if (word.StartsWith(layerPrefix))
                                    layer = LayerMask.NameToLayer(word.Substring(layerPrefix.Length));
                                else if (char.IsUpper(word[0]))
                                    name = word;
                                else
                                    Debug.LogWarning("Unknown word: " + word);
                                break;
                            }
                        }
                    }

                    Assert.IsNotNull(name);
                    
                    if (isPrefab) {
                        var prefab = TryFindPrefab(name);
                        if (prefab)
                            instance = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, root)).transform;
                        else
                            Debug.LogWarning("Prefab not found: " + name);
                    }

                    if (!instance)
                        instance = Instantiate(transform, root);

                    instance.name = meshName;

                    instance.transform.position = transform.position;
                    instance.transform.rotation = transform.rotation;
                    instance.transform.localScale = transform.localScale;

                    if (shouldBeStatic)
                        instance.gameObject.isStatic = true;
                    
                    if (layer is { } value)
                        instance.gameObject.layer = value;
                    
                    if (withMeshCollider) {
                        var meshCollider = instance.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = meshFilter.sharedMesh;
                    }
                }

                for (var i = 0; i < transform.childCount; i++)
                    Process(transform.GetChild(i));
            }

            Process(blueprint);
        }
    }
}