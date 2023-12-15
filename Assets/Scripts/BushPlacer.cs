using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class BushPlacer : MonoBehaviour {

    public BushTester bushTester;
    public MeshFilter meshFilter;
    public VoronoiRenderer voronoiRenderer;
    public List<(Vector3 position, Quaternion rotation, Vector3 scale)> bushes = new();
    [Command] public Vector2 bushSizeRange = new(.25f, 1.5f);
    public InstancedMeshRenderer bushRenderer;
    [Command]
    public float bushDensityPerUnit = 1;
    [Command]
    public int bushSeed = 0;
    
    [Command]
    public void PlaceBushes() {
        var origin = meshFilter.sharedMesh ? meshFilter.sharedMesh.bounds.min.ToVector2() : Vector2.zero;
        var uvs = voronoiRenderer.Distribute(voronoiRenderer.bushMaskRenderTexture, voronoiRenderer.worldSize, bushDensityPerUnit, bushSeed).ToList();

        bushes.Clear();
        foreach (var uv in uvs) {

            var position2d = origin + uv * voronoiRenderer.worldSize;
            
            
            
            var scale = Vector3.one * Mathf.Lerp(bushSizeRange[0], bushSizeRange[1], Random.value);
            if (PlaceOnTerrain.TryRaycast(position2d, out var hit)) {
                
                using(Draw.ingame.WithDuration(5))
                    Draw.ingame.Line(hit.point, hit.point+Vector3.up);
                
                var position3d = hit.point;
                var rotation = (-hit.normal).ToRotation(Random.value * 360);
                if (bushTester) {
                    bushTester.transform.position = hit.point;
                    bushTester.transform.localScale = scale;
                    for (var tries = 0; tries < 5; tries++) {
                        bushTester.transform.rotation = rotation;
                        if (!bushTester.IntersectsRoads()) {
                            bushes.Add((position3d, rotation, scale));
                            break;
                        }
                        rotation = (-hit.normal).ToRotation(Random.value * 360);
                    }
                }
                else 
                    bushes.Add((position3d, rotation, scale));
            }
        }

        UpdateBushRenderer();
    }

    public void UpdateBushRenderer() {
        if (bushRenderer) {
            if (bushRenderer.transformList) {
                Destroy(bushRenderer.transformList);
                bushRenderer.transformList = null;
            }

            if (bushes.Count > 0) {
                bushRenderer.transformList = ScriptableObject.CreateInstance<TransformList>();
                bushRenderer.transformList.name = "Bushes";
                bushRenderer.transformList.matrices = bushes.Select(bush => Matrix4x4.TRS(bush.position, bush.rotation, bush.scale)).ToArray();
                bushRenderer.transformList.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
                bushRenderer.ResetGpuBuffers();
            }
        }
    }
}