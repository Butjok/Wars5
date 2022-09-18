Shader "Custom/Stone"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Grass ("_Grass", Color) = (1,1,1,1)
        _DarkGrass ("_DarkGrass", Color) = (1,1,1,1)
        _Wheat ("_Wheat", Color) = (1,1,1,1)
        _YellowGrass ("_YellowGrass", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Splat ("_Splat", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:instanced_rendering_vertex addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex,_Splat;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _Bounds;
        half2 _Flip;
        float3 _Grass,_DarkGrass,_Wheat,_YellowGrass;

        struct InstancedRenderingAppdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;

            uint inst : SV_InstanceID;
        };
        #include "Assets/Shaders/InstancedRendering.cginc"

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            //o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;

            float2 splatUv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw);
            if (_Flip.x > .5)
                splatUv.x = 1 - splatUv.x;
            if (_Flip.y > .5)
                splatUv.y = 1 - splatUv.y;

            float3 splat = tex2D(_Splat, splatUv);

            o.Albedo = _Grass;
            o.Albedo = lerp(o.Albedo, _DarkGrass, splat.r);
            o.Albedo = lerp(o.Albedo, _Wheat, splat.g);
            o.Albedo = lerp(o.Albedo, _YellowGrass, splat.b);

            //o.Albedo=splat;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
