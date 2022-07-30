// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Hole"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"  "ForceNoShadowCasting" = "True"  }
        ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha:fade 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
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

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            half2 screenPos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
            half4 center = UnityObjectToClipPos(float3(0,0,0));
            center /= center.w;
            center = (center +1)/2;
            center.y = 1-center.y;
            
            center.xy *= _ScreenParams.xy;
            
            half screenSpaceRadius = 50;

            
            half dist=length(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(0,0,0,1)));
            screenSpaceRadius = 500 / dist;

//clip(smoothstep(screenSpaceRadius, screenSpaceRadius+10, length(screenPos - center.xy))-.5);
            
            o.Albedo.rgb = 1;//
            o.Alpha =smoothstep(screenSpaceRadius, screenSpaceRadius+100/ dist, length(screenPos - center.xy));
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
