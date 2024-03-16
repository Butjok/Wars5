using System;
using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUi4 : MonoBehaviour {

    public RectTransform root;
    public float hiddenYPosition = -500;
    public Easing.Name easing = Easing.Name.Linear;
    public float duration = .5f;

    public Image leftPortrait, rightPortrait;
    public TMP_Text speakerNameText, speechText;
    public float typingSpeed = 1000;
    public Image spaceBarKeyImage;

    public Color natalieColor = Color.white;
    public Color vladanColor = Color.yellow;
    public Color jamesWillisColor = Color.blue;
    public Color ljubisaDragovicColor = Color.red;

    public UiCircle circle;
    public Image wasdImage;
    public Image movement2;

    public bool ShowSpaceBarKey {
        set => spaceBarKeyImage.enabled = value;
    }

    public class CirclePulsation : IDisposable {
        public UiCircle circle;
        public CirclePulsation(UiCircle circle, Vector3 position, Vector2 radius) {
            this.circle = circle;
            circle.StartCoroutine(Animation(position, radius));
        }
        public IEnumerator Animation(Vector3 position, Vector2 radius) {
            var startTime = Time.time;
            circle.image.enabled = true;
            circle.position = position;
            while (true) {
                circle.radius = Mathf.Lerp(radius[0], radius[1], Mathf.PingPong(Time.time*2, 1));
                yield return null;
            }

            Dispose();
        }
        public void Dispose() {
            circle.position = null;
            circle.image.enabled = false;
        }
    }

    public CirclePulsation PulsateCircle(Vector3 position, Vector2 radius) {
        return new CirclePulsation(circle, position, radius);
    }
    public CirclePulsation PulsateCircle(Vector3 position) {
        return new CirclePulsation(circle, position, new Vector2(1.15f, 1.35f));
    }

    public Color GetPersonColor(PersonName personName) {
        return personName switch {
            PersonName.Natalie => natalieColor,
            PersonName.Vladan => vladanColor,
            PersonName.JamesWillis => jamesWillisColor,
            PersonName.LjubisaDragovic => ljubisaDragovicColor,
            _ => Color.white
        };
    }

    [Command]
    public void SetSpeaker(PersonName? speaker, bool clear = true) {
        if (speaker is { } actualSpeaker) {
            speakerNameText.text = Persons.GetFirstName(actualSpeaker);
            speakerNameText.color = GetPersonColor(actualSpeaker);
        }
        else
            speakerNameText.text = "";

        if (clear)
            ClearText();
    }
    [Command]
    public void ClearText() {
        speechText.text = "";
    }

    public IEnumerator typingCoroutine;

    [Command]
    public void TypeText(string text, string start = "") {
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = TextTypingAnimation(start, text);
        StartCoroutine(typingCoroutine);
    }

    public IEnumerator TextTypingAnimation(string start, string text) {
        speechText.text = start;
        var startTime = Time.time;
        var duration = text.Length / typingSpeed;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            var length = Mathf.RoundToInt(t * text.Length);
            speechText.text = start + text.Substring(0, length);
            yield return null;
        }

        speechText.text = start + text;
    }

    public enum Side {
        Left,
        Right
    }

    public void SetPortrait(Side side, PersonName? personName, Mood mood = default) {
        var target = side == Side.Left ? leftPortrait : rightPortrait;
        Sprite portrait;
        if (personName is { } actualPersonName && (portrait = Persons.TryGetPortrait(actualPersonName, mood))) {
            target.enabled = true;
            target.sprite = portrait;
        }
        else
            target.enabled = false;
    }

    public IEnumerator moveAnimation;

    [Command]
    public void Show() {
        if (moveAnimation != null) {
            StopCoroutine(moveAnimation);
            moveAnimation = null;
        }

        root.gameObject.SetActive(true);
        moveAnimation = MoveAnimation(root.anchoredPosition.y, 0, false);
        StartCoroutine(moveAnimation);
    }

    [Command]
    public void Hide() {
        if (moveAnimation != null) {
            StopCoroutine(moveAnimation);
            moveAnimation = null;
        }

        moveAnimation = MoveAnimation(root.anchoredPosition.y, hiddenYPosition, true);
        StartCoroutine(moveAnimation);
    }

    public IEnumerator MoveAnimation(float startY, float targetY, bool disableOnFinish) {
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            var y = Mathf.Lerp(startY, targetY, Easing.Dynamic(easing, t));
            var position = root.anchoredPosition;
            position.y = y;
            root.anchoredPosition = position;
            yield return null;
        }

        var finalPosition = root.anchoredPosition;
        finalPosition.y = targetY;
        root.anchoredPosition = finalPosition;
        if (disableOnFinish)
            root.gameObject.SetActive(false);
    }
}