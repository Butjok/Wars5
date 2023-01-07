using Butjok.CommandLine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlogPostStackMachineVisualizationStackValue : MonoBehaviour {


    public RectTransform RectTransform => GetComponent<RectTransform>();

    public Tween Move(RectTransform from, RectTransform to, float speed, Ease ease) {
        if (RectTransform.anchoredPosition != from.anchoredPosition)
            RectTransform.anchoredPosition = from.anchoredPosition;
        var distance = Vector2.Distance(RectTransform.anchoredPosition, to.anchoredPosition);
        var time = distance / speed;
        return RectTransform.DOAnchorPos(to.anchoredPosition, time).SetEase(ease);
    }
    [Command]
    public Tween Fade(float alpha, float duration, Ease ease) {
        var sequence = DOTween.Sequence();
        sequence.Append(GetComponent<Image>().DOFade(alpha, duration).SetEase(ease));
        sequence.Join(GetComponentInChildren<TMP_Text>().DOFade(alpha, duration).SetEase(ease));
        return sequence;
    }

    public string Text {
        set {
            GetComponentInChildren<TMP_Text>().text = value;
        }
    }


    [Command]
    public void Punch() {
        transform.DOPunchPosition(Vector3.one, 1);
    }
}