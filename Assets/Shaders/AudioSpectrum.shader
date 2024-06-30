Shader "Hidden/AudioSpectrum"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Range(0.1, 10)) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _AudioSpectrum[256];
            float _Scale;
            float4 _Color;

            fixed4 frag(v2f i) : SV_Target {
                float2 uv = i.uv;
                float val = _AudioSpectrum[int(uv.x * 256)];
                float height = val * _Scale;
                bool isIn = uv.y < height;
                return _Color * isIn;
            }
            ENDCG
        }
    }
}