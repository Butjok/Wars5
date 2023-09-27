Shader "Unlit/TerrainEdge"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Min ("_Min", Vector) = (0,0,0,1)
		_Size ("_Size", Vector) = (1,1,0,1)
		_OutsideColor ("_OutsideColor", Color) = (0,0,0,1)
		_BorderColor ("_BorderColor", Color) = (1,1,1,1)
		_BorderSmoothness ("_BorderSmoothness", Float) = 0.1
		_BorderColor2 ("_BorderColor2", Color) = (1,1,1,1)
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
			};

			struct v2f
			{
				float3 worldPos : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float2 _Min, _Size;
			float4 _OutsideColor, _BorderColor, _BorderColor2;
			float _BorderSmoothness;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul( unity_ObjectToWorld, v.vertex );
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			#include "Assets/Shaders/SDF.cginc"
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = 0;
				// apply fog
				
				float2 position = i.worldPos.xz;
				col = 0;
				col.rg = position;
				float2 center = _Min + _Size/2;
				float dist = length(position - center);
				dist = sdfBox(position-center, _Size/2);
				
				//col.rgb = dist;
				float borderMask =  smoothstep(.5 + _BorderSmoothness, .5, dist);
				col.rgb = lerp(_OutsideColor, _BorderColor.rgb, borderMask);
				
				col.rgb = lerp(col.rgb, _BorderColor2, smoothstep(.55, .5125, dist));
				//col.rgb += (sin(dist*10)+1)/2/10;
				
				return col;
			}
			ENDCG
		}
	}
}