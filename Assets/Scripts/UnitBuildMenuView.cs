using System;
using TMPro;
using UnityEngine;

public class UnitBuildMenuView : MonoBehaviour {

	public TMP_Text credits;
	
	public bool Visible {
		set => gameObject.SetActive(value);
	}
	public Player Player {
		set => credits.text = value.credits.ToString();
	}
	
}