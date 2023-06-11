using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneTemplateProcessor : AssetPostprocessor {

    public bool TryFindPrefab(string name, string directory, out GameObject prefab) {

        var matches = AssetDatabase.FindAssets("t:Prefab " + name, new[] { directory }).Select(guid => (guid, path: AssetDatabase.GUIDToAssetPath(guid))).ToList();
        matches.RemoveAll(m => Path.GetFileNameWithoutExtension(m.path) != name);
        Assert.IsTrue(matches.Count <= 1, $"Multiple prefabs named '{name}'");
        if (matches.Count == 1) {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(matches[0].path);
            return true;
        }

        matches = AssetDatabase.FindAssets("t:Model " + name, new[] { directory }).Select(guid => (guid, path: AssetDatabase.GUIDToAssetPath(guid))).ToList();
        matches.RemoveAll(m => Path.GetFileNameWithoutExtension(m.path) != name);
        Assert.IsTrue(matches.Count <= 1, $"Multiple models named '{name}'");
        if (matches.Count == 1) {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(matches[0].path);
            return true;
        }

        prefab = null;
        return false;
    }

    private void OnPostprocessModel(GameObject model) {

        if (!model.name.StartsWith("SceneTemplate"))
            return;
        
        var directory = Path.GetDirectoryName(assetPath);

        void Traverse(Transform node) {

            var meshRenderer = node.GetComponent<MeshRenderer>();
            var meshFilter = node.GetComponent<MeshFilter>();
            var mesh = meshFilter ? meshFilter.sharedMesh : null;

            if (meshRenderer && meshFilter && mesh && mesh.name != model.name && TryFindPrefab(mesh.name, directory, out var prefab)) {
                var instance = PrefabUtility.InstantiatePrefab(prefab, node.parent) as GameObject;
                instance.name = node.name;
                instance.transform.localPosition = node.localPosition;
                instance.transform.localRotation = node.localRotation;
                instance.transform.localScale = node.localScale;
                var index = node.transform.GetSiblingIndex();
                Object.DestroyImmediate(node.gameObject);
                Debug.Log(instance.name);
                instance.transform.SetSiblingIndex(index);
            }
            else
                for (var i = 0; i < node.childCount; i++)
                    Traverse(node.GetChild(i));
        }

        Traverse(model.transform);
    }
}