using UnityEngine;

public class FpsCounter : MonoBehaviour {

	public static string[] texts = new string[500];
	static FpsCounter() {
		for (var i = 0; i < texts.Length; i++)
			texts[i] = i.ToString();
	}

	public GUISkin skin;
	public GUIContent content;
	
	public void OnGUI() {
		if (!Debug.isDebugBuild || !Game.ShowDebugGui)
			return;
		GUI.skin = skin;
		var fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
		var max = texts.Length - 1;
		var clamped = Mathf.Clamp(fps, 0, max);
		var text = texts[clamped];
		var style = GUI.skin.label;
		content.text = text;
		var size = style.CalcSize(content);
		GUI.Label(new Rect(new Vector2(Screen.width-size.x, 0), size), text);
	}
}