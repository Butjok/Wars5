using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(LayoutElement))]
public class AboutRow : MonoBehaviour {
    public TMP_Text left, right;
    public LayoutElement layoutElement;
    private void Awake() {
        layoutElement = GetComponent<LayoutElement>();
        Assert.IsTrue(layoutElement);
    }
    private void Update() {
        layoutElement.preferredHeight = Mathf.Max(left.preferredHeight, right.preferredHeight);
    }
}
