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
_FillTex ("_FillTex", 2D) = "white" {}

    }
    SubShader {
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_FillTex;
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color, _BorderColor, _FillColor;
half _BorderOffset, _BorderThinkness, _BorderSharpness, _FillOffset, _FillSharpness,_CutoffDistance;
sampler2D _FillTex;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            
            float dist = IN.color.r;
            
            clip( _CutoffDistance  -dist);
            
            float border2 = smoothstep(_BorderThinkness+_BorderSharpness,_BorderThinkness-_BorderSharpness,abs(dist - _BorderOffset));
            o.Albedo = border2;
            
            float fill = smoothstep(_FillSharpness,-_FillSharpness,dist - _FillOffset);
            o.Albedo += fill/2;
            
            fixed4 final = max(border2*_BorderColor, fill* _FillColor*tex2D(_FillTex, IN.uv_FillTex));  
            
            o.Albedo = final.rgb;
            o.Alpha = final.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}