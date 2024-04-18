Shader "Unlit/FrostedGlass1"
{
    Properties
    {
        _Size ("Blur", Vector) = (4,4,0,0)
        [HideInInspector] _MainTex ("Masking Texture", 2D) = "white" {}
        _AdditiveColor ("Additive Tint color", Color) = (0, 0, 0, 0)
        _MultiplyColor ("Multiply Tint color", Color) = (1, 1, 1, 1)
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    Category
    {

        // We must be transparent, so other objects are drawn before this one.
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque"
        }


        SubShader
        {
            // Vertical blur
            GrabPass
            {
                "_VBlur"
            }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 uvmain : TEXCOORD1;
                    float2 screenPosition : TEXCOORD2;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;

                float N21(float2 p) {
                    p = frac(p * float2(123.34, 345.45));
                    p += dot(p, p + 34.345);
                    return frac(p.x * p.y);
                }

                v2f vert(appdata_t v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.screenPosition = ComputeScreenPos(o.vertex);

#if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
#else
					float scale = 1.0;
#endif

                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;

                    o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);


                    return o;
                }

                sampler2D _VBlur;
                float4 _VBlur_TexelSize;
                float2 _Size;
                float4 _AdditiveColor;
                float4 _MultiplyColor;

                half4 frag(v2f i) : COLOR {
                    half3 sum = half3(0, 0, 0);
                    half alpha = tex2D(_MainTex, i.uvmain).a * _Color;
                    half radius = alpha * _Size.x;


                    const float numSamples = 64;
                    float a = N21(i.uvgrab.xy) * 6.283185;
                    for (float j = 0; j < numSamples; j++) {
                        float2 offset = float2(cos(a), sin(a)) * radius;
                        float d = frac(sin((j + 1) * 564.) * 5424.);
                        d = sqrt(d);
                        offset *= d;
                        float2 uv = saturate(i.uvgrab.xy + offset);
                        sum += tex2Dlod(_VBlur, float4(uv, 0, 0)).rgb;
                        a++;
                    }
                    sum /= numSamples;

                    float yDistance = min(1 - i.screenPosition.y, i.screenPosition.y)/.5;
                    float xDistance = min(1 - i.screenPosition.x, i.screenPosition.x)/.5;
                    float dist = pow(1 - xDistance * yDistance, 25);

                    return half4(lerp(1, 0.5, dist) * (sum * lerp(float3(1, 1, 1), _MultiplyColor, alpha) + alpha * _AdditiveColor), 1);
                }
                ENDCG
            }
        }
    }
}