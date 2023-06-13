using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UiLineRenderer : MaskableGraphic {

    [SerializeField] private List<Vector2> points = new();
    public void ClearPoints() {
        points.Clear();
        SetVerticesDirty();
    }
    public void AddPoint(Vector2 point) {
        points.Add(point);
        SetVerticesDirty();
    }
    public void RemovePoint(int index) {
        Assert.IsTrue(index >= 0 && index < points.Count);
        points.RemoveAt(index);
        SetVerticesDirty();
    }
    public Vector2 this[int index] {
        get {
            Assert.IsTrue(index >= 0 && index < points.Count);
            return points[index];
        }
        set {
            Assert.IsTrue(index >= 0 && index < points.Count);
            if (value != points[index]) {
                points[index] = value;
                SetVerticesDirty();
            }
        }
    }
    public int PointsCount => points.Count;

    public float thickness = 5;

    public Texture texture;
    public override Texture mainTexture => texture ? texture : s_WhiteTexture;

    protected override void OnValidate() {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper) {

        UIVertex V(Vector2 position, float uvx, float uvy) {
            return new UIVertex {
                color = Color.white,
                position = position,
                uv0 = new Vector2(uvx, uvy)
            };
        }

        vertexHelper.Clear();
        for (var i = 0; i < points.Count - 1; i++) {

            var a = points[i];
            var b = points[i + 1];

            var ab = b - a;
            var n = new Vector2(-ab.y, ab.x).normalized * thickness / 2;
            var ap = a + n;
            var am = a - n;
            var bp = b + n;
            var bm = b - n;
            vertexHelper.AddQuad(V(ap, .5f, 1), V(bp, .5f, 1), V(bm, .5f, 0), V(am, .5f, 0));

            {
                var p = ap;
                var m = am;
                var mp = p - m;
                var n2 = new Vector2(-mp.y, mp.x).normalized * thickness / 2;
                vertexHelper.AddQuad(V(p + n2, 0, 1), V(p, .5f, 1), V(m, .5f, 0), V(m + n2, 0, 0));
            }
            {
                var p = bp;
                var m = bm;
                var mp = p - m;
                var n2 = new Vector2(-mp.y, mp.x).normalized * thickness / 2;
                vertexHelper.AddQuad(V(p, .5f, 1), V(p - n2, 1, 1), V(m - n2, 1, 0), V(m, .5f, 0));
            }
        }
    }
}