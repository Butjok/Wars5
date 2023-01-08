Shader "Custom/TileAreaTest" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

_BorderOffset("_BorderOffset", Range(-2, 2)) = 1
_BorderThinkness("_BorderThinkness", Range(-2, 2)) = 1
_BorderSharpness("_BorderSharpness", Range(-2, 2)) = 1

_FillOffset("_FillOffset", Range(-2, 2)) = 1
_FillSharpness("_FillSharpness", Range(-2, 2)) = 1

[HDR] _BorderColor ("_BorderColor", Color) = (1,1,1,1)
[HDR] _FillColor ("_FillColor", Color) = (1,1,1,1)

_CutoffDistance("_CutoffDistance", Range(-2, 2)) = 1

    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color, _BorderColor, _FillColor;
half _BorderOffset, _BorderThinkness, _BorderSharpness, _FillOffset, _FillSharpness,_CutoffDistance;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float dist = IN.color.r;
            
            clip( _CutoffDistance  -dist);
            
            float border2 = smoothstep(_BorderThinkness+_BorderSharpness,_BorderThinkness-_BorderSharpness,abs(dist - _BorderOffset));
            o.Albedo = border2;
            
            float fill = smoothstep(_FillSharpness,-_FillSharpness,dist - _FillOffset);
            o.Albedo += fill/2;
            
            o.Albedo=0;
            o.Emission = border2*_BorderColor;
            o.Emission += fill* _FillColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}