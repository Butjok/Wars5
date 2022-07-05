Shader "Custom/Unit"
{
    Properties
    {
        _PlayerColor ("Player Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Occlusion ("Occlusion", 2D) = "white" {}
        _Roughness ("Roughness", Range(0,1)) = 0.0
        _Normal ("Normal", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex,_Occlusion,_Normal;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Roughness;
        half _Metallic;
        fixed4 _PlayerColor;
        half _Selected;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _PlayerColor;
            o.Albedo = c.rgb;
            //o.Albedo=tex2D (_Normal, IN.uv_MainTex);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = 1-_Roughness;
            o.Alpha = c.a;
            o.Occlusion=tex2D (_Occlusion, IN.uv_MainTex);
            o.Normal=UnpackNormal(tex2D (_Normal, IN.uv_MainTex));

            o.Emission=saturate( _Selected);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
