using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;

public class MissionObjectivesUi : MonoBehaviour {

	public TMP_Text text;
	public List<string> bulletPoints = new();
	public string bulletPointFormat = "• {0};";
	public string separator = "\n";

	[Command]
	public bool Enable {
		get => gameObject.activeSelf;
		set => gameObject.SetActive(value);
	}
	public void OnEnable() {
		if (text)
			text.text = string.Join(separator, bulletPoints.Select(line => string.Format(bulletPointFormat, line)));
	}
}