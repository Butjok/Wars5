using System.Collections;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePortraitStack : MonoBehaviour {

    public Vector2 size = new(100, 100);
    public Vector2 delta = new(100, 0);
    public List<Image> images = new();

    public Queue<IEnumerator> animationQueue = new();

    public Vector2 PositionOnAxis(float position) {
        return (Vector2)transform.position + delta * position;
    }

    public IEnumerator MoveAnimation(Image image, Vector2 delta, float duration = .5f, bool destroy = false, float? fadeTo = null) {
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

    public Image image;

    [Command]
    public void AddImage() {

        var go = new GameObject();
        go.transform.SetParent(transform);
        var image = go.AddComponent<Image>();
        image.rectTransform.sizeDelta = size;
        var from = PositionOnAxis(images.Count + 1);
        images.Add(image);

        image.transform.position = from;
        image.color = new Color(1, 1, 1, 0);
        animationQueue.Enqueue(MoveAnimation(image, -delta, fadeTo: 1));
    }

    [Command]
    public void RemoveImage(int index) {
        var down = Vector3.Cross(Vector3.forward, delta);
        if (down.y > 0)
            down = -down;
        animationQueue.Enqueue(MoveAnimation(images[index], down, .5f, fadeTo: 0, destroy: true));
        images.RemoveAt(index);
        for (var i = index; i < images.Count; i++) {
            var item = images[i];
            animationQueue.Enqueue(MoveAnimation(item, -delta, .33f));
        }
    }

    private void Update() {
        while (animationQueue.TryPeek(out var coroutine)) {
            if (coroutine.MoveNext())
                break;
            animationQueue.Dequeue();
        }
    }
}