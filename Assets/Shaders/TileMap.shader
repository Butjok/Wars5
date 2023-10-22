Shader "Unlit/TileMap"
{
	Properties
	{
		_TileMap ("Texture", 2D) = "black" {}
		_Alpha ("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
		LOD 100
        
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
			Offset -1, -1
			Fog { Mode Off }
			ColorMask RGB
			AlphaTest Greater .01
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
				float3 worldPos : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			sampler2D _TileMap;
			fixed4x4 _WorldToTileMap;
			float _Alpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = mul(_WorldToTileMap, float4(i.worldPos, 1)).xz;
				if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
					discard;
				fixed4 color = tex2D(_TileMap, uv);
				color.a = _Alpha;
				return color;
			}
			ENDCG
		}
	}
}