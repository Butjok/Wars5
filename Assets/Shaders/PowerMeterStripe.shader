// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/PowerMeterStripe"
{
Properties
{
    [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
_FillerMask ("_FillerMask", 2D) = "white" {}
_Color ("Tint", Color) = (1,1,1,1)

[HDR] _UnfilledColor ("_UnfilledColor", Color) = (1,1,1,1)
[HDR] _FilledColor ("_FilledColor", Color) = (1,1,1,1)
[HDR] _FullColor ("_FullColor", Color) = (1,1,1,1)

_StencilComp ("Stencil Comparison", Float) = 8
_Stencil ("Stencil ID", Float) = 0
_StencilOp ("Stencil Operation", Float) = 0
_StencilWriteMask ("Stencil Write Mask", Float) = 255
_StencilReadMask ("Stencil Read Mask", Float) = 255

_ColorMask ("Color Mask", Float) = 15
_PulseFrequency ("_PulseFrequency", Float) = 1
_PulseAmplitude ("_PulseAmplitude", Float) = 1

_Progress ("_Progress", Range(0,1)) = 0
_ProgressSmoothness ("_ProgressSmoothness", Float) = 1
_Tiling ("_Tiling", Float) = 1

[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
}

SubShader
{
    Tags
{
    "Queue"="Transparent"
    "IgnoreProjector"="True"
    "RenderType"="Transparent"
    "PreviewType"="Plane"
    "CanUseSpriteAtlas"="True"
}

Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp]
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}

Cull Off
Lighting Off
ZWrite Off
ZTest [unity_GUIZTestMode]
Blend SrcAlpha OneMinusSrcAlpha
ColorMask [_ColorMask]

Pass
{
    Name "Default"
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"
    #include "UnityUI.cginc"

    #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
    #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

    struct appdata_t
    {
        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 vertex   : SV_POSITION;
        fixed4 color    : COLOR;
        float2 texcoord  : TEXCOORD0;
        float4 worldPosition : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex,_FillerMask;
    fixed4 _Color, _UnfilledColor, _FilledColor, _FullColor;
    fixed4 _TextureSampleAdd;
    float4 _ClipRect;
    float4 _MainTex_ST;
float _Progress,_Tiling,_ProgressSmoothness,_PulseFrequency,_PulseAmplitude;

    v2f vert(appdata_t v)
{
    v2f OUT;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
    OUT.worldPosition = v.vertex;
    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

    OUT.color = v.color * _Color;
    return OUT;
}

fixed4 frag(v2f IN) : SV_Target
{
    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;


    float2 uv = IN.texcoord;
    float2 tiledUv = uv * float2(_Tiling, 1);
    float filled = _Progress <= 0 ? 0 : smoothstep(_Progress - _ProgressSmoothness, _Progress + _ProgressSmoothness, uv.x);

    color = tex2D(_MainTex, tiledUv);
    half fillerMask = tex2D(_FillerMask, tiledUv).r;
    
    if (_Progress >= 1) {
        _FilledColor = _FullColor;
        half pulseIntensity = (sin(_Time.y * _PulseFrequency) / 2 + .5) * _PulseAmplitude;
        _FilledColor *= 1 + pulseIntensity;
    }
    color.rgb += fillerMask * filled * _FilledColor;
    color.rgb += fillerMask * (1-filled) * _UnfilledColor;
    color *=   IN.color;

    return color;
}
ENDCG
}
}
}