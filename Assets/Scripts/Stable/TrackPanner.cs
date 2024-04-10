using System;
using UnityEngine;

[ExecuteInEditMode]
public class TrackPanner : MonoBehaviour {

    public Renderer[] renderers = Array.Empty<Renderer>();
    public MaterialPropertyBlock materialPropertyBlock;
    public Vector3? lastPosition;
    public float speed = 1;
    public int materialIndex = -1;
    public float advancement;

    public void Update() {
        if (lastPosition is { } actualLastPosition) {
            var delta = transform.position - actualLastPosition;
            advancement += Vector3.Dot(delta, transform.forward) * speed;
            materialPropertyBlock ??= new MaterialPropertyBlock();
            materialPropertyBlock.SetVector("_Offset", new Vector4(0, advancement));
            foreach (var renderer in renderers)
                if (materialIndex == -1)
                    renderer.SetPropertyBlock(materialPropertyBlock);
                else
                    renderer.SetPropertyBlock(materialPropertyBlock, materialIndex);
        }

        lastPosition = transform.position;
    }
}