using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Object = UnityEngine.Object;

public class PostProcessingFade : MonoBehaviour { 

    /*public static PostProcessingFade fader;

    public static void Fade( Color target, float duration) {

        if (!fader) {
            var go = new GameObject(nameof(PostProcessingFade));
            Object.DontDestroyOnLoad(go);
            fader = go.AddComponent<PostProcessingFade>();
        }

        fader.StopAllCoroutines();
        fader.StartCoroutine(FadeAnimation(PostProcessing..value, target, duration, color => colorParameter.value = color));
    }

    public static IEnumerator FadeAnimation(Color a, Color b, float duration, Action<Color> action) {
        var start = Time.time;
        action(a);
        while (Time.time < start + duration) {
            yield return null;
            action(Color.Lerp(a, b, (Time.time - start) / duration));
        }
        yield return null;
        action(b);
    }*/
}