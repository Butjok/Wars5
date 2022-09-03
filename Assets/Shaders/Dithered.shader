Shader "Custom/Hideable"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _VisibilityF("Visibility", Range(0,1)) = 1.0
        _Mask("Visibility Mask", 2D) = "gray" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
 
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
 
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
 
        sampler2D _MainTex;
        sampler2D _Mask;
 
        struct Input
        {
            float2 uv_MainTex;
            float4 customScreenPos;
        };
 
        // custom vertex function to calculate the screen pos
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.customScreenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
        }
 
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _VisibilityF;
 
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
 
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
 
            float2 screenUV = IN.customScreenPos.xy / IN.customScreenPos.w;
            // proper aspect ratio correction rather than the hard coded 16:9 aspect you had
            screenUV.x *= _ScreenParams.x / _ScreenParams.y;
            screenUV *= 1;
 
            #if defined(SHADOWS_DEPTH) && defined(UNITY_PASS_SHADOWCASTER)
            if (!any(unity_LightShadowBias)) {
                // most likely the camera depth texture
                clip(_VisibilityF - tex2D(_Mask, screenUV).r);
            }
            #endif
            #if !defined(UNITY_PASS_SHADOWCASTER)
            clip(_VisibilityF - tex2D(_Mask, screenUV).r);
            #endif

            o.Albedo = tex2D(_Mask, screenUV).rrr;
        }
        ENDCG
    }
    FallBack "Standard"
}