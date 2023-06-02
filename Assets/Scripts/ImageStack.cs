using System;
using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ImageStack : MonoBehaviour {

    public Vector2 size = new(100, 100);
    public Vector2 delta = new(100, 0);
    public List<Image> images = new();
    [FormerlySerializedAs("scale")] public Vector2 scalingFactor = new(1, 1);

    public Queue<IEnumerator> queue = new();

    public Vector2 PositionOnAxis(float position) {
        return (Vector2)transform.position + delta * position;
    }
    public bool IsCompleted(IEnumerator coroutine) {
        return !queue.Contains(coroutine);
    }

    public static IEnumerator MovementCoroutine(Image image, Vector2 delta, float duration = .5f, bool destroy = false, float? fadeTo = null) {
        var startTime = Time.time;
        Vector2 from = image.transform.position;
        var to = from + delta;
        var fadeFrom = image.color.a;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(Easing.Name.InOutQuad, t);
            image.transform.position = Vector2.Lerp(from, to, t);
            if (fadeTo is { } value) {
                var color = image.color;
                color.a = Mathf.Lerp(fadeFrom, value, t);
                image.color = color;
            }
            yield return null;
        }
        if (destroy)
            Destroy(image.gameObject);
        else
            image.transform.position = to;
    }

    [Command]
    public (Image image, IEnumerator coroutine) AddImage(string name = "Image") {

        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var image = go.AddComponent<Image>();
        image.rectTransform.sizeDelta = size;
        var from = PositionOnAxis(images.Count + 1);
        images.Add(image);

        var scale = image.transform.localScale;
        scale.Scale(scalingFactor);
        image.transform.localScale = scalingFactor;

        image.transform.position = from;
        image.color = new Color(1, 1, 1, 0);

        var coroutine = MovementCoroutine(image, -delta, fadeTo: 1);
        queue.Enqueue(coroutine);
        return (image, coroutine);
    }

    [Command]
    public IEnumerator RemoveImage(int index) {

        var down = Vector3.Cross(Vector3.forward, delta);
        if (down.y > 0)
            down = -down;

        var coroutine = MovementCoroutine(images[index], down, .5f, fadeTo: 0, destroy: true);
        queue.Enqueue(coroutine);

        images.RemoveAt(index);
        for (var i = index; i < images.Count; i++) {
            var item = images[i];
            coroutine = MovementCoroutine(item, -delta, .33f);
            queue.Enqueue(coroutine);
        }

        return coroutine;
    }

    public IEnumerator RemoveImage(Image image) {
        var index = images.IndexOf(image);
        Assert.AreNotEqual(-1, index);
        return RemoveImage(index);
    }

    [Command]
    public IEnumerator Clear() {
        IEnumerator coroutine = null;
        while (images.Count > 0)
            coroutine = RemoveImage(0);
        return coroutine;
    }

    private void Update() {
        while (queue.TryPeek(out var coroutine)) {
            if (coroutine.MoveNext())
                break;
            queue.Dequeue();
        }
    }
}