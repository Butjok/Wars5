Shader "Custom/water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("_Scale", Float) = 1
        _Speed ("_Speed", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"  "ForceNoShadowCasting" = "True" }
        ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard   vertex:vert alpha:fade 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic,_Scale,_Speed;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        half Wave(half2 uv){
            half distance =tex2Dlod (_MainTex,  float4(uv, 0.0, 0.0)).r;
            half wave=sin(distance*_Scale + _Speed*_Time.y)/2+.5;
            return wave;
        }
        half WaveMask(half2 uv){
            half distance =tex2Dlod (_MainTex,  float4(uv, 0.0, 0.0)).r;
            half wavesMask = smoothstep(.5, .0, distance);
            return wavesMask;
        }
        
        void vert(inout appdata_full data){
            half distance =tex2Dlod (_MainTex,  float4(data.texcoord.xy, 0.0, 0.0)).r;
            half wave=sin(distance*_Scale + _Speed*_Time.y + data.vertex.x*5 + data.vertex.z*5)/2+.5;
            half wavesMask = smoothstep(.5, .0, distance);
            data.vertex.y += wavesMask*wave*.005;
                                          
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half distance =tex2D (_MainTex, IN.uv_MainTex).r;
            half wave=sin(distance*_Scale + _Speed*_Time.y)/2+.5;
            half wavesMask = smoothstep(.5, .0, distance);
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = _Color;// wavesMask*wave*wavesMask*wave;// wavesMask*wave;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = lerp(.5,.99,smoothstep(0, .5, distance));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
