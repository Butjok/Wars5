using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class PolygonShaderTest : MonoBehaviour {

    public const int pointsCapacity = 128;

    public Material material;
    private readonly List<Vector4> list = new(pointsCapacity);

    public Mesh meshFrom, meshTo;

    private void OnEnable() {
        UpdateMaterial();
        ResetAnimation();
    }

    [Command]
    public void UpdateMaterial() {

        void FillList(IEnumerable<Vector2> points) {
            list.Clear();
            foreach (var point in points)
                list.Add(point);
            var padLength = list.Capacity - list.Count;
            for (var i = 0; i < padLength; i++)
                list.Add(Vector2.zero);
            Assert.AreEqual(pointsCapacity, list.Count);
        }

        Assert.IsTrue(material);
        Assert.IsTrue(meshFrom);
        Assert.IsTrue(meshTo);
        Assert.AreEqual(meshFrom.vertexCount, meshTo.vertexCount);

        material.SetInt("_Count", meshFrom.vertexCount);
        FillList(meshFrom.vertices.Select(vertex => transform.TransformPoint(vertex).ToVector2()));
        material.SetVectorArray("_From", list);
        FillList(meshTo.vertices.Select(vertex => transform.TransformPoint(vertex).ToVector2()));
        material.SetVectorArray("_To", list);
    }

    [Command]
    public void ResetAnimation() {
        material.SetFloat("_StartTime", float.MaxValue);
    }

    [Command]
    public void PlayAnimation() {
        material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
    }
}