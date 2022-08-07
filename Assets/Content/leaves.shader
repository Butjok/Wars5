Shader "Custom/leaves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint ("_Tint", 2D) = "white" {}        
        _Normal ("_Normal", 2D) = "normal" {}
        _Dist ("_Dist", 2D) = "white" {}
        _SSS ("_SSS", 2D) = "white" {}
        _WithSSS ("_WithSSS", 2D) = "white" {}
        _WithoutSSS ("_WithoutSSS", 2D) = "white" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _GlobalOcclusion ("_GlobalOcclusion", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _SSSIntensity ("_SSSIntensity", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex,_Occlusion,_SSS,_Dist,_Normal,_Tint,_GlobalOcclusion,_WithSSS,_WithoutSSS;

        struct Input {
            float2 uv_MainTex ;
            float2 uv2_GlobalOcclusion ;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic,_SSSIntensity;
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
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            clip(c.a-.5);

            half globalOcclusion =tex2D (_GlobalOcclusion, IN.uv2_GlobalOcclusion).r; 
            half localOcclusion= tex2D (_Occlusion, IN.uv_MainTex).r;
            
            half2 position = IN.worldPos.xz; 
            // Albedo comes from a texture tinted by color
            c.rgb = tex2D (_Tint,  position/5)  * localOcclusion;

            half dist = tex2D (_Dist, IN.uv_MainTex).r;
            half distI = smoothstep(.1, 0, dist);
            
            o.Albedo = tex2D (_Tint, IN.uv_MainTex) * tex2D (_Occlusion, IN.uv_MainTex) * (1-_SSSIntensity);

            o.Occlusion = 1;//globalOcclusion*localOcclusion *  (1-_SSSIntensity);
            
            o.Emission=tex2D (_SSS, IN.uv2_GlobalOcclusion)*_SSSIntensity;
            //o.Albedo = 0;
            //o.Emission = tex2D (_SSS, IN.uv2_SSS);
            
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = lerp(.25, .5, pow(tex2D (_Occlusion, IN.uv_MainTex),.5)) * (globalOcclusion);
            //o.Alpha = c.a;
            

            o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
