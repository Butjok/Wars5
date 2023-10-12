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
		[HDR]_BorderColor2 ("_BorderColor2", Color) = (1,1,1,1)
		_Thickness ("_Thickness", Float) = .5
		_GridRange ("_GridRange", Vector) = (1,1,0,1)
		_CenterSize ("_CenterSize", Vector) = (1,1,0,1)
		_UnderlayColor ("_UnderlayColor", Color) = (1,1,1,1)
		_UnderlayColor2 ("_UnderlayColor2", Color) = (1,1,1,1)
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
			float4 _OutsideColor, _BorderColor, _BorderColor2, _UnderlayColor, _UnderlayColor2, _SoilColor;
			float _BorderSmoothness;
			fixed _Thickness;
			fixed2 _GridRange;
			float4 _CenterSize;
			
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
				_Size -= .33;
				dist = sdfBox(position-center, _Size/2);
				
				//col.rgb = dist;
				float borderMask =  smoothstep(.6 + _BorderSmoothness, .6, dist);
				col.rgb = lerp(_OutsideColor, _BorderColor.rgb, borderMask*borderMask);
				
				col.rgb = lerp(col.rgb, _BorderColor2, smoothstep(_Thickness+.525, _Thickness+.5125, dist));
				//col.rgb += (sin(dist*10)+1)/2/10;
				
				half2 dst = abs(i.worldPos.xz-.5 - round(i.worldPos.xz-.5)) / fwidth(i.worldPos.xz);
				half dst2 = min(dst.x, dst.y);
				half gridMask = smoothstep(_GridRange.x, _GridRange.y, dist);
				col.rgb = lerp(col, col*.85, gridMask*smoothstep(.75, .5, dst2));
				
				
				float3 direction = normalize(i.worldPos - _WorldSpaceCameraPos);
				if (direction.y < 0) {
					float enter = (_WorldSpaceCameraPos.y ) / (-direction.y);
					float3 hitPoint = _WorldSpaceCameraPos + enter * direction;
					float distance = sdfBox(hitPoint.xz - center, _Size/2);
					col.rgb = lerp(col.rgb, _UnderlayColor, smoothstep(.025, 0, sdfBox(hitPoint.xz - center, _Size/2 + 1)-.125));
					float3 soilColor = lerp(_SoilColor/2, _SoilColor, smoothstep(5, 0, distance+2.5));
					col.rgb = lerp(col.rgb, soilColor, smoothstep(.05, .025, distance -.65));
				}
				
				
				return col;
			}
			ENDCG
		}
	}
}