using System.IO;
using System.Linq;
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

public class PasteTransformsWindow : EditorWindow {

    private DefaultAsset selectedAsset;

    [MenuItem("Tools/Import Transforms")]
    public static void OpenWindow() {
        var window = GetWindow<PasteTransformsWindow>();
        window.titleContent = new GUIContent("Import Transforms");
        window.Show();
    }

    private void OnGUI() {
        selectedAsset = EditorGUILayout.ObjectField(selectedAsset, typeof(DefaultAsset), false) as DefaultAsset;
        if (GUILayout.Button("Process")) {

            using (TextReader textReader = File.OpenText(AssetDatabase.GetAssetPath(selectedAsset))) {

                var root = GLTFRoot.Deserialize(textReader);

                Node FindParent(Node node) {
                    Assert.IsTrue(node.Mesh is { Id: >= 0 });
                    foreach (var n in root.Nodes)
                        if (n.Children != null && n.Children.Any(nodeId => nodeId.Id == node.Mesh.Id))
                            return n;
                    return null;
                }

                foreach (var node in root.Nodes)
                    if (node.Mesh != null) {

                        node.GetUnityTRSProperties(out var localPosition, out var localRotation, out var localScale);
                        var matrix = Matrix4x4.TRS(localPosition, localRotation, localScale);
                        for (var parent = FindParent(node); parent != null; parent = FindParent(parent)) {
                            parent.GetUnityTRSProperties(out var parentPosition, out var parentRotation, out var parentScale);
                            matrix = Matrix4x4.TRS(parentPosition, parentRotation, parentScale) * matrix;
                        }

                        using (Draw.editor.WithDuration(5))
                        using (Draw.editor.WithMatrix(matrix))
                            Draw.editor.SolidBox(float3.zero, new float3(2, 2, 2));
                    }
            }
        }
    }
}