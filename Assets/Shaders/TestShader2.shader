Shader "Unlit/TestShader2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 lightProbe : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			#include "UnityStandardUtils.cginc"
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.lightProbe = ShadeSHPerVertex(v.normal, 0);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
	
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed3 col = 0;
				
				col += saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
				col += half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				//col += ;
				//col += ShadeSHPerPixel(i.normal, 0, i.worldPos);
				//col += float3(1,1,1) * saturate(dot(i.normal, normalize(_WorldSpaceLightPos0.xyz)));
				//col += i.normal; 
				
				return float4(col,1);
			}
			ENDCG
		}
	}
}