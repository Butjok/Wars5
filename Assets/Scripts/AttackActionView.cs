using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class AttackActionView : MonoBehaviour {

    public UnitView2 debugTarget;
    public IUiBoundPoints target;
    [SerializeField] private RectTransform frame;
    [SerializeField] private TMP_Text damageText;

    public int Damage {
        set => damageText.text = $"-{value}";
    }

    private void LateUpdate() {
        Assert.IsTrue(debugTarget);
        if (debugTarget)
            target = debugTarget;
        frame.gameObject.SetActive(frame.TryEncapsulate(target, out _));
    }
}