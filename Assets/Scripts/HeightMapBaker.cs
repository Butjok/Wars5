using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class HeightMapBaker : MonoBehaviour {

    public MeshFilter meshFilter;
    public int pixelsPerUnit = 16;
    public RenderTexture renderTexture;
    public Camera camera;
    public Material material;
    public Texture2D texture;
    public Material[] targetMaterials = { };

    public MeshFilter[] additionalMeshFilters = { };

    [Command]
    public void Bake() {
        if (!meshFilter)
            return;
        var mesh = meshFilter.sharedMesh;
        if (!mesh)
            return;

        var size = mesh.bounds.size.ToVector2().RoundToInt();
        Debug.Log(size);
        Debug.Log(mesh. bounds.size);
        Assert.IsTrue(Mathf.Approximately(mesh.bounds.size.x, size.x));
        Assert.IsTrue(Mathf.Approximately(mesh.bounds.size.z, size.y));
        renderTexture = new RenderTexture(size.x * pixelsPerUnit, size.y * pixelsPerUnit, 32, RenderTextureFormat.RFloat);
        if (!renderTexture.Create()) {
            var created = renderTexture.Create();
            Assert.IsTrue(created);
        }

        var cameraGameObject = new GameObject("Camera");
        camera = cameraGameObject.AddComponent<Camera>();
        camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        camera.transform.position = mesh.bounds.center + 100 * Vector3.up;
        camera.orthographic = true;
        camera.orthographicSize = (float)size.y / 2;
        camera.aspect = (float)size.x / size.y;
        camera.cullingMask = 1 << LayerMask.NameToLayer("Heightmap");
        camera.targetTexture = renderTexture;
        camera.enabled = false;

        void RenderMesh(Mesh mesh, Matrix4x4 transform) {
            for (var i = 0; i < mesh.subMeshCount; i++)
                Graphics.DrawMesh(mesh, transform, material, LayerMask.NameToLayer("Heightmap"), camera, i, null, false, false);    
        }
        
        RenderMesh(meshFilter.sharedMesh, Matrix4x4.identity);
        foreach (var meshFilter in additionalMeshFilters)
            RenderMesh(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix);
        
        camera.Render();

        texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RFloat, false);
        texture.filterMode = FilterMode.Bilinear;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        var worldToHeightMap = Matrix4x4.TRS(mesh.bounds.min, Quaternion.identity, mesh.bounds.size).inverse;
        foreach (var targetMaterial in targetMaterials) {
            targetMaterial.SetTexture("_TerrainHeight", texture);
            targetMaterial.SetMatrix("_WorldToTerrainHeightUv", worldToHeightMap);
        }

        Destroy(cameraGameObject);
        camera = null;
    }

    public void Start() {
        Bake();
    }

    private void OnDestroy() {
        if (renderTexture)
            Destroy(renderTexture);
        if (texture)
            Destroy(texture);
    }
}