Shader "Custom/MissionLink" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Length ("_Length", Range(0,100)) = 0.0
        _ActiveLength ("_ActiveLength", Range(0,100)) = 0.0
        _FillRadius ("_FillRadius", Range(0,5)) = 0.0
_HeadRadius ("_HeadRadius", Vector) = (0,0,0,0)
        _Speed ("_Speed", Range(0,5)) = 0.0
        _Active ("_Active", Range(0,1)) = 0.0
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
        half _Metallic;
        fixed4 _Color;
        float _Length, _ActiveLength, _Speed;
float _FillRadius,_StartTime=-10000;
half _Active;
half4 _HeadRadius;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
            
            
            float position = _Length * IN.uv_MainTex.x;
            _ActiveLength = (_Time.y - _StartTime) * _Speed;
            
            half fill = smoothstep(_ActiveLength + _FillRadius, _ActiveLength - _FillRadius, position);
//            half head = smoothstep(_ActiveLength + _HeadRadius.x, _ActiveLength - _HeadRadius.y, position);
            
            clip(fill-.5);

            o.Emission = fill*5;
o.Alpha = fill;
//            o.Emission.r += head*50;
        }
        ENDCG
    }
    FallBack "Diffuse"
}