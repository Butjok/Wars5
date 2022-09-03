Shader "Custom/Difference"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Target ("_Target", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex,_Target;
            half fit ;
            half _Multiplier;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 uv = i.uv;
                uv *= _ScreenParams.xy;
                if (fit == 0)
                    uv = (uv + uv - _ScreenParams.xy) / _ScreenParams.y;
                else if (fit == 1)
                    uv = (uv + uv - _ScreenParams.xy) / _ScreenParams.x;

                //;
                
                fixed4 screen = tex2D(_MainTex, i.uv);
                //fixed4 target = tex2D(_Target, uv);

                fixed4 result = 0;
                fixed2 oldUv = uv;
                uv = (uv + 1) / 2;
                fixed4 target = tex2D(_Target, uv);
                
                result.rg=oldUv;;
                return (target-screen) * (oldUv.x < 0 ? 1 : -1)*5;
            }
            ENDCG
        }
    }
}
