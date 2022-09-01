using UnityEngine;

public class FpsCounter : MonoBehaviour {

	public static string[] texts = new string[200];
	static FpsCounter() {
		for (var i = 0; i < texts.Length; i++)
			texts[i] = i.ToString();
	}

	public GUISkin skin;
	
	public void OnGUI() {
		GUI.skin = skin;
		var fps = Mathf.RoundToInt(1f / Time.deltaTime);
		var max = texts.Length - 1;
		var clamped = Mathf.Clamp(fps, 0, max);
		GUILayout.Label(texts[clamped]);
	}
}