using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class HpText : MonoBehaviour {
	public TMP_Text text;
	public void Reset() {
		text = GetComponent<TMP_Text>();
	}
}