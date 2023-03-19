using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class AttackActionView : MonoBehaviour {

	public UnitView target;
	[SerializeField] private RectTransform frame;
	[SerializeField] private TMP_Text damageText;

	public int Damage {
		set => damageText.text = $"-{value}";
	}

	private void LateUpdate() {
		if (target)
			frame.gameObject.SetActive(frame.TryEncapsulate(target.UiBoundPoints, out _));
	}
}