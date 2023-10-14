using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CameraFadingRenderer), PostProcessEvent.AfterStack, "Custom/CameraFading")]
public sealed class CameraFading : PostProcessEffectSettings {
    [Range(0f, 100f), Tooltip("Square size.")]
    public FloatParameter squareSize = new() { value = 20f, overrideState = true };
    [Range(0f, 10), Tooltip("Circle smoothness.")]
    public FloatParameter smoothness = new() { value = 0.5f, overrideState = true };
    [Range(0, 1), Tooltip("Progress.")]
    public FloatParameter progress = new() { value = 1f, overrideState = true };
    [Range(0, 1), Tooltip("Progress smoothness.")]
    public FloatParameter progressSmoothness = new() { value = 0.25f, overrideState = true };
    [Range(-1, 1), Tooltip("Y contribution.")]
    public FloatParameter yContribution = new() { value = 0f, overrideState = true };
    [Range(0, 1), Tooltip("Invert.")]
    public BoolParameter invert = new() { overrideState = true };
}

public sealed class CameraFadingRenderer : PostProcessEffectRenderer<CameraFading> {
    public const string shaderName = "Hidden/Custom/CameraFading";
    public override void Render(PostProcessRenderContext context) {
        var sheet = context.propertySheets.Get(Shader.Find(shaderName));
        sheet.properties.SetFloat("_SquareSize", settings.squareSize);
        sheet.properties.SetFloat("_Smoothness", settings.smoothness);
        sheet.properties.SetFloat("_Progress", settings.progress);
        sheet.properties.SetFloat("_ProgressSmoothness", settings.progressSmoothness);
        sheet.properties.SetFloat("_YContribution", settings.yContribution);
        sheet.properties.SetFloat("_Invert", settings.invert ? 1 : 0);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}

