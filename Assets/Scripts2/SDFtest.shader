Shader "Custom/SDFtest" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
_LineDistance("Mayor Line Distance", Range(0, 2)) = 1
_LineThickness("Mayor Line Thickness", Range(0, 0.1)) = 0.05
_BorderOffset("_BorderOffset", Range(-2, 2)) = 1
_BorderThinkness("_BorderThinkness", Range(-2, 2)) = 1
_BorderSharpness("_BorderSharpness", Range(-2, 2)) = 1
_OutsideSmoothness("_OutsideSmoothness", Range(-2, 2)) = 1
_OutsideOffset("_OutsideOffset", Range(-2, 2)) = 1
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
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

float _LineDistance;
float _LineThickness;
float _BorderOffset, _BorderThinkness, _BorderSharpness,_OutsideSmoothness,_OutsideOffset;

float4 _Bounds;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            
            float2 uv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw - _Bounds.xy);
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, uv);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            float dist = c.r;
            float distanceChange = fwidth(dist) * 0.5;
            float majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
            float majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);
            
            float border = smoothstep(_BorderThinkness-_BorderSharpness,_BorderThinkness+_BorderSharpness,abs(dist - _BorderOffset));
            float outside = smoothstep(0, _OutsideSmoothness, dist - _OutsideOffset);
            
            o.Albedo = 0;
            o.Albedo.r = majorLines;
            o.Albedo.g = border;
            o.Albedo.b = outside;
        }
        ENDCG
    }
    FallBack "Diffuse"
}