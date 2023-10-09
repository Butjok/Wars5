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
		_BorderOffset ("_BorderOffset", Float) = 0.1
		[HDR]_BorderColor2 ("_BorderColor2", Color) = (1,1,1,1)
		_Thickness ("_Thickness", Float) = .5
		
		_SoilColor ("_SoilColor", Color) = (1,1,1,1)
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
			float4 _OutsideColor, _BorderColor, _BorderColor2, _SoilColor;
			float _BorderSmoothness, _BorderOffset;
			fixed _Thickness;
			
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
				
				col.a = 1;
				col.rgb = lerp(_SoilColor.rgb, _OutsideColor.rgb, smoothstep(-.5, -.6, i.worldPos.y));
				
				return col;
			}
			ENDCG
		}
	}
}