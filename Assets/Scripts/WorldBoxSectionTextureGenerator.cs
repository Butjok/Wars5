using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class WorldBoxSectionTextureGenerator : MonoBehaviour {

    public BoxCollider boxCollider;

    private void OnDrawGizmos() {

        if (!boxCollider)
            return;

        var up = Vector3.up * 3;
        {
            var p = GetWorldPosition(boxCollider, new Vector2(0, 0)).ToVector3();
            Gizmos.DrawLine(p, p + up);
        }
        {
            var p = GetWorldPosition(boxCollider, new Vector2(1, 0)).ToVector3();
            Gizmos.DrawLine(p, p + up);
        }
        {
            var p = GetWorldPosition(boxCollider, new Vector2(0, 1)).ToVector3();
            Gizmos.DrawLine(p, p + up);
        }
        {
            var p = GetWorldPosition(boxCollider, new Vector2(1, 1)).ToVector3();
            Gizmos.DrawLine(p, p + up);
        }
    }


    public BoxCollider[] boxes = { };

    public Texture2D texture;
    public int size = 2048;

    public Material material;
    [FormerlySerializedAs("uniformName")] public string textureUniformName = "_Distance";
    public string boundsUniformName = "_Bounds";

    private void Start() {
        SetUniforms();
    }

    [ContextMenu(nameof(GenerateTexture))]
    public void GenerateTexture() {
        
        var texelUvSize = Vector2.one / size;
        texture = new Texture2D(size, size, TextureFormat.RFloat, false, true);

        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++) {

            var pixel = new Vector2Int(x, y);
            var uv = (pixel + new Vector2(.5f, .5f)) * texelUvSize;
            var worldPosition = GetWorldPosition(boxCollider, uv);

            var minDistance = float.MaxValue;
            foreach (var box in boxes) {

                var bounds = box.bounds;
                var center = bounds.center.ToVector2();
                var extents = bounds.extents.ToVector2();
                
                //Debug.DrawLine(worldPosition.ToVector3(), worldPosition.ToVector3()+Vector3.up, Color.white, 1);

                var distance = (worldPosition - center).SignedDistanceBox(extents);
                minDistance = Mathf.Min(minDistance, distance);
            }

            texture.SetPixel(x, y, new Color(minDistance, 0, 0));
        }

        texture.Apply();
        
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(texture, "Assets/Scenes/Distance.asset");
        AssetDatabase.SaveAssets();
#endif
    }

    public void SetUniforms() {
        if (material) {
            material.SetTexture(textureUniformName, texture);

            var min = GetWorldPosition(boxCollider, Vector2.zero);
            var max = GetWorldPosition(boxCollider, Vector2.one);
            var bounds = new Vector4(min.x, min.y, max.x, max.y);
            material.SetVector(boundsUniformName, bounds);
        }
    }

    public static Bounds GetBounds(BoxCollider boxCollider) {
        var oldEnabled = boxCollider.enabled;
        boxCollider.enabled = true;
        var bounds = boxCollider.bounds;
        boxCollider.enabled = oldEnabled;
        return bounds;
    }

    public static Vector2 GetUv(BoxCollider boxCollider, Vector2 worldPosition) {

        var bounds = GetBounds(boxCollider);
        var min = bounds.min.ToVector2();
        var max = bounds.max.ToVector2();

        return (worldPosition - min) / (max - min);
    }

    public static Vector2 GetWorldPosition(BoxCollider boxCollider, Vector2 uv) {

        var bounds = GetBounds(boxCollider);
        var min = bounds.min.ToVector2();
        var size = bounds.size.ToVector2();

        return min + size * uv;
    }
}