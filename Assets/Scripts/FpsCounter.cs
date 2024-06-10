using UnityEngine;

public class FpsCounter : MonoBehaviour {

    public static string[] texts = new string[500];
    static FpsCounter() {
        for (var i = 0; i < texts.Length; i++)
            texts[i] = i.ToString();
    }

    public GUISkin skin;
    public GUIContent content;

    public string GetString(int value) {
        return texts[Mathf.Clamp(value, 0, texts.Length - 1)];
    }
    public void Draw(string text) {
        var style = GUI.skin.label;
        content.text = text;
        var size = style.CalcSize(content);
        GUI.Label(new Rect(new Vector2(Screen.width - size.x, 0), size), text);
    }

    public const int fpsCaptureFramesCount = 30;
    public const float fpsAverageTimeSeconds = 0.5f;
    public const float fpsStep = fpsAverageTimeSeconds / fpsCaptureFramesCount;

    public int index;
    public float[] history = new float[fpsCaptureFramesCount];
    public float average, last;

    public int GetFPS() {
        var fpsFrame = Time.deltaTime;
        if (fpsFrame == 0)
            return 0;

        if (Time.time - last > fpsStep) {
            last = Time.time;
            index = (index + 1) % fpsCaptureFramesCount;
            average -= history[index];
            history[index] = fpsFrame / fpsCaptureFramesCount;
            average += history[index];
        }

        return Mathf.RoundToInt(1.0f / average);
    }

    public void OnGUI() {
        if (!Debug.isDebugBuild)
            return;
        GUI.skin = skin;

        var fps = GetFPS();
        var max = texts.Length - 1;
        var clamped = Mathf.Clamp(fps, 0, max);
        var text = texts[clamped];
        Draw(text);
    }
}