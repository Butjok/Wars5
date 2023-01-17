Shader "Unlit/TriggerUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
Blend One One

Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color.a / 3;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

    int flags = round(i.color.a);
    fixed4 result = float4(0,0,0,1);

    result += ((flags & 1) != 0 ? 1 : 0) * float4(1,0,0,0);
    result += ((flags & 2) != 0 ? 1 : 0) * float4(0,1,0,0);
    result += ((flags & 4) != 0 ? 1 : 0) * float4(0,0,1,0);

                return result*10;
            }
            ENDCG
        }
    }
}