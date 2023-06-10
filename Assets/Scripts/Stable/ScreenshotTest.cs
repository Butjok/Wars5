using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotTest : MonoBehaviour {

    public RenderTexture texture;
    public RawImage image;

    [Command()]
    public void TakeScreenshot() {

        texture = new RenderTexture(Screen.width, Screen.height, 0);
        ScreenCapture.CaptureScreenshotIntoRenderTexture(texture);
        image.enabled = true;
        image.texture = texture;

        if (image.material)
            image.material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
    }
}