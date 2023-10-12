Shader "Unlit/test-shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CenterSize ("_CenterSize", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Assets/Shaders/SDF.cginc"

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _CenterSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = i.worldPos;
                
                float3 cameraPosition = _WorldSpaceCameraPos;
                float3 position = i.worldPos;
                float3 direction = normalize(position - cameraPosition);
                if (direction.y < 0) {
                	float enter = cameraPosition.y / (-direction.y);
                	float3 hitPoint = cameraPosition + enter * direction;
                	float distance = sdfBox(hitPoint.xz - _CenterSize.xy, _CenterSize.zw);
                	col.rgb = smoothstep(0, .025, distance);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
