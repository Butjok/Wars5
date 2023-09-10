using System.Collections;
using Butjok.CommandLine;
using Unity.Mathematics;
using UnityEngine;

public class Dissolve : MonoBehaviour {

    public Vector2 thresholdRange = new(30, -5);
    public MaterialPropertyBlock materialPropertyBlock;
    public Renderer renderer;
    public float3 rotation = new(0, 0, 0);
    public Vector2 noiseScaleRange = new(2, 10);

    public void Awake() {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    [Command]
    public void Animate(float duration) {
        StopAllCoroutines();
        StartCoroutine(Animation(duration));
    }

    public IEnumerator Animation(float duration) {
        var start = Time.time;
        var startRotation = transform.rotation;
        while (Time.time < start + duration) {
            var t = (Time.time - start) / duration;
            Threshold = Mathf.Lerp(thresholdRange[0], thresholdRange[1], t);
            NoiseScale = Mathf.Lerp(noiseScaleRange[0], noiseScaleRange[1], t);
            transform.rotation = Quaternion.Lerp(startRotation, startRotation * Quaternion.Euler(rotation), t);
            yield return null;
        }
        transform.rotation = startRotation;
    }

    [Command]
    public void ResetAnimation() {
        Threshold = thresholdRange[0];
    }

    [Command]
    public float Threshold {
        set {
            materialPropertyBlock.SetFloat("_Threshold", value);
            if (renderer)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    [Command]
    public float NoiseScale {
        set {
            materialPropertyBlock.SetFloat("_NoiseScale", value);
            if (renderer)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}