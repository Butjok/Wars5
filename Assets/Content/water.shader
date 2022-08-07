Shader "Custom/water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("_Scale", Float) = 1
        _Alpha ("_Alpha", Float) = 1
        _Speed ("_Speed", Float) = 1
        _Normal ("_Normal", 2D) = "normal" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"  "ForceNoShadowCasting" = "True" }
        ZWrite Off
        Blend One One
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard   vertex:vert  alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        sampler2D _MainTex,_Normal;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness,_Alpha;
        half _Metallic,_Scale,_Speed;
        fixed4 _Color;
        float4 _Normal_ST;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        half Wave(half2 uv){
            half distance =tex2Dlod (_MainTex,  float4(uv, 0.0, 0.0)).r;
            half wave=min(sin(distance*_Scale + _Speed*_Time.y), sin(distance*_Scale + _Speed*_Time.y + 3.1415)) + 1;//   sin(distance*_Scale + _Speed*_Time.y)/2+.5;
            return wave;
        }
        half WaveMask(half2 uv){
            half distance =tex2Dlod (_MainTex,  float4(uv, 0.0, 0.0)).r;
            half wavesMask = smoothstep(.5, .0, distance);
            return wavesMask;
        }
        
        void vert(inout appdata_full data){
            half distance =tex2Dlod (_MainTex,  float4(data.texcoord.xy, 0.0, 0.0)).r;
            half wave = min ( sin(distance*_Scale + _Speed*_Time.y/* + data.vertex.x*5 + data.vertex.z*5*/),
                              sin(distance*_Scale + _Speed*_Time.y/* + data.vertex.x*5 + data.vertex.z*5*/ + 3.1415) );
            half wavesMask = smoothstep(.5, .0, distance);
            data.vertex.y += wavesMask*wave*.0025;
                                          
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half distance =tex2D (_MainTex, IN.uv_MainTex).r;
            half wave=sin(distance*_Scale + _Speed*_Time.y)/2+.5;
            half wavesMask = smoothstep(.75, .0, distance);
            float2 position = IN.worldPos.xz;
            
            // Albedo comes from a texture tinted by color
            fixed3 normal3 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position/10 + float2(_Time.x*.25, _Time.x*.25)),_Normal)));
            fixed3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position + float2(_Time.x*4, 0)),_Normal)));
            fixed3 normal2 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position - float2(0, _Time.x*7)),_Normal)));
            o.Albedo = _Color;// normal.rgb;// wavesMask*wave*wavesMask*wave;// wavesMask*wave;

            half wave2 = min ( sin(distance*_Scale + _Speed*_Time.y),
                              sin(distance*_Scale + _Speed*_Time.y + 3.1415) )+1;

           // float3 ddxPos = ddx(wave2);
           //float3 ddyPos = ddy(wave2)  * _ProjectionParams.x;
           //float3 normal3 = normalize( cross(ddxPos, ddyPos));
            
            //o.Albedo=normal3;

float3 targetNormal = normal3 + normal + normal2;
            
            o.Normal = normalize(normal+normal2);
            o.Normal = normalize(lerp(float3(0,0,1), targetNormal, lerp(.05,1,wavesMask)));
            //o.Normal = targetNormal;
            //o.Normal = float3(0,0,1);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha=1;
            o.Alpha = lerp(.75,.9,smoothstep(0, .5, distance));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
