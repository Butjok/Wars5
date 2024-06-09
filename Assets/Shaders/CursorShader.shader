Shader "Unlit/CursorShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TerrainHeight ("Terrain Height", 2D) = "black" {}
		[HDR]_Tint ("Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		
		Tags
		{
			"Queue" = "Transparent+1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
		
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest Always
			//Offset -1, -1
			//Fog { Mode Off }
			//ColorMask RGB
			//AlphaTest Greater .01
			Blend SrcAlpha OneMinusSrcAlpha
		
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex, _TerrainHeight;
			float4 _MainTex_ST, _Tint;
			fixed4x4 _WorldToTerrainHeightUv;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float2 terrainHeightUv = mul(_WorldToTerrainHeightUv, float4(worldPos, 1)).xz;
				float terrainHeight = tex2Dlod(_TerrainHeight, float4(terrainHeightUv, 0, 0)).r;
				//worldPos.z = terrainHeight;

				v.vertex *= 1 + abs(frac(_Time.y*2)-0.5)*.1;
				v.vertex.z -= terrainHeight;

				o.vertex = UnityObjectToClipPos(v.vertex);
				
				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Tint;
				return col;
			}
			ENDCG
		}
	}
}