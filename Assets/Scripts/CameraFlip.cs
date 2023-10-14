using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CameraFlipRenderer), PostProcessEvent.AfterStack, "Custom/CameraFlip")]
public sealed class CameraFlip : PostProcessEffectSettings {
}

public sealed class CameraFlipRenderer : PostProcessEffectRenderer<CameraFlip> {
    public const string shaderName = "Hidden/Flip";
    public override void Render(PostProcessRenderContext context) {
        var sheet = context.propertySheets.Get(Shader.Find(shaderName));
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}