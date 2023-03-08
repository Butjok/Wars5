Shader "Custom/TextFrame3d" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
_Size ("_Size", Vector) = (0,0,0,0)
_Margin ("_Margin", Float) = .1
_Smoothstep ("_Smoothstep", Vector) = (0,0,0,0)
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
        };

        half _Glossiness;
        half _Metallic,_Margin;
        fixed4 _Color,_Size,_Smoothstep;

half sdfBox(half2 p, half2 size)
{
    half2 d = abs(p) - size;
    return length(max(d, half2(0,0))) + min(max(d.x, d.y), 0.0);
}

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            o.Albedo = _Color;
            
            
            float2 position = (IN.uv_MainTex-0.5) * _Size.xy/2;
            float distance = sdfBox(position, _Size.xy/4 - _Margin) - _Margin/4;
            float mask = 1-smoothstep(_Smoothstep.x, _Smoothstep.y, distance);
            clip(mask-.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}