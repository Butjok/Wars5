Shader "Custom/MinimapUnit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("_Normal", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _Infantry ("_Infantry", 2D) = "white" {}
        _Recon ("_Recon", 2D) = "white" {}
        _LightTank ("_LightTank", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow  

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex,_Normal,_Infantry,_Recon,_LightTank;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #define EPSILON .1
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            clip(tex2D (_MainTex, IN.uv_MainTex).r-.5);

            half id = IN.color.a;
            half infantry = abs(id-0) < EPSILON;
            half recon = abs(id-8) < EPSILON;
            half lightTank = abs(id-9) < EPSILON;

            half mask = 0;
            mask += infantry * tex2D(_Infantry, IN.uv_MainTex).a;
            mask += recon * tex2D(_Recon, IN.uv_MainTex).a;
            mask += lightTank * tex2D(_LightTank, IN.uv_MainTex).a;
            
            o.Albedo = lerp(IN.color, float3(0,0,0), mask);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;

            o.Normal=UnpackNormal(tex2D(_Normal,IN.uv_MainTex));

            clip(mask-.5);
            o.Albedo = IN.color;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
