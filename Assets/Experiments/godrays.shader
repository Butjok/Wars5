// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/godrays"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Lod ("LOD", Range(0, 10)) = 1
        _Color ("Color", Color) = (1,1,1,1)
        _DesiredThickness ("Desired Thickness", Range(0.01, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        // blend multiplicative
        Blend DstColor Zero
        ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float4x4 _WorldToCookie;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float _Lod;
            float _DesiredThickness;

            float LightAtLocation(float3 pos) {
                float2 cookieUv = mul(_WorldToCookie, float4(pos, 1)).xy + 0.5;
                float col = tex2Dlod(_MainTex, float4(cookieUv,0,_Lod)).r;
                return col;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = float4(0,0,0,1);
                
                // sample the texture
                float3 worldPos = i.worldPos;
                float3 toCamera = _WorldSpaceCameraPos - worldPos;
                float distanceToCamera = length(toCamera);
                float3 towardsCamera = normalize(toCamera);
                
                col.rgb = LightAtLocation(worldPos);

             //   float desiredThickness = 10;
                float3 fullTowardsCamera = towardsCamera / (dot(towardsCamera, float3(0,1,0)) / _DesiredThickness);
                float fullLength = length(fullTowardsCamera);

                float3 location = worldPos;
                float sumIntensity=0;
                float steps = 15;
                float3 step = fullTowardsCamera / steps;
                float stepLength = length(step);
                float accum = 0;
                float distanceWent=0;
                for (int i = 1; i <= steps; i++) {
                    location = worldPos + step * i;
                    distanceWent += stepLength;
                    if (distanceWent > distanceToCamera) {
                        break;
                    }
                    float len = distanceWent;
                    float intensity = 1-len / fullLength;
                    accum += intensity*LightAtLocation(location);
                    sumIntensity += intensity;
                }
                accum /= sumIntensity;

                //return 1/dot(towardsCamera, desiredThickness);

                //return LightAtLocation(worldPos);
                
                return  float4(lerp(_Color, float3(1,1,1), accum), 1);
            }
            ENDCG
        }
    }
}
