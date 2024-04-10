Shader "Custom/rough2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "white" {}
        _Metallic ("Metallic", 2D) = "white" {}
    	_Normal ("Normal", 2D) = "bump" {}
       [HDR] _Emissive ("Emissive", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _Roughness, _TileMask, _Normal, _Metallic;
        fixed4x4 _TileMask_WorldToLocal;
        fixed4 _Emissive;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;

        #include "Utils.cginc"
        #include "Assets/Shaders/SDF.cginc"
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #include "Assets/Shaders/ClassicNoise.cginc"

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = tex2D(_Metallic, IN.uv_MainTex).r;
            o.Smoothness = 1-tex2D(_Roughness, IN.uv_MainTex).r;
            o.Alpha = c.a;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
