// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Unlit/UnlitLeaves"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("_Alpha", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD1;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex,_Alpha;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv0 = v.uv0;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                clip(tex2D(_Alpha, i.uv).r-.50);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);


                
                return col;
            }
            ENDCG
        }
    }
}
