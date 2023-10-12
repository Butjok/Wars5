Shader "Custom/Voronoi"
{
	Properties
	{
		_Size ("_Size", Vector) = (1,1,1,1)
		_CellScale ("_CellScale", Float) = 1
		_FieldScale ("_FieldScale", Float) = 1
		_RadiusNoiseScale ("_RadiusNoiseScale", Float) = 1
		
		_BlurRadiusNoiseScale ("_BlurRadiusNoiseScale", Float) = 1
		_BlurRadiusRange ("_BlurRadiusRange", Vector) = (.25, 5, .75, 1)
		
		_Power ("_Power", Float) = 1
		_Radius ("_Radius", Float) = 0.01
		_MainTex ("_MainTex", 2D) = "white" {}
		
		_Thresholds ("_Thresholds", Vector) = (.25, 5, .75, 1)
		_Smoothness ("_Smoothness", Float) = 0.05
		
		_RadiusRange ("_RadiusRange", Vector) = (.25, 5, .75, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			Name "Voronoi"
		
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
			
			float _CellScale, _Power, _FieldScale;
			fixed2 _Size;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p,float2(117.12,341.7)),dot(p,float2(269.5,123.3))))*43458.5453);
			}
			float random (float2 st) {
                return frac(sin(dot(st.xy,
                                     float2(12.9898,78.233)))*
                    43758.5453123);
            }
			
			#include "Assets/Shaders/ClassicNoise.cginc"
			
			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv;
				float2 position = uv * _Size;
				int2 id = floor(position * _CellScale);
				float minDist = 9999;
				float2 closestPoint = position;
				for (int y = -1; y <= 1; y++)
					for (int x = -1; x <= 1; x++) {
						int2 neighborId = id + int2(x, y);
						float2 cellPoint = (neighborId + random2(neighborId)) / _CellScale;
						float2 difference = abs(cellPoint - position);
						float dist = pow(pow(difference.x, _Power) + pow(difference.y, _Power), 1 / _Power);
						if (dist < minDist)
						{
							minDist = dist;
							closestPoint = cellPoint;
						}
					}
				//return float4(closestPoint, 0, 1);

				float noise = 0;
				noise += (ClassicNoise(float3(closestPoint * _FieldScale,0)) + 1) / 2;
				noise += (ClassicNoise(float3(closestPoint * _FieldScale * 2,0)) + 1) / 2 * .5;
				noise += (ClassicNoise(float3(closestPoint * _FieldScale * 4,0)) + 1) / 2 * .25;
				noise /= 1.75;
				return float4(noise, noise, noise, 1);
			}
		ENDCG
		}
		
		Pass
		{
			Name "Blur"
			
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
			
			sampler2D _MainTex;
			fixed2 _Size;
			float _Radius, _Smoothness, _RadiusNoiseScale, _BlurRadiusNoiseScale;
			float4 _Thresholds;
			float2 _RadiusRange, _BlurRadiusRange;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			#include "Assets/Shaders/ClassicNoise.cginc"
			#include "Assets/Shaders/TerrainMask.cginc"
			
			fixed4 frag(v2f i) : SV_Target
			{
				float2 position = i.uv * _Size;
				float blurRadius = lerp(_BlurRadiusRange.x, _BlurRadiusRange.y, (ClassicNoise(float3(position * _BlurRadiusNoiseScale, 0)) + 1) / 2);
				
				const int samples = 16;
				const float angleStep = 3.1415926 * 2.0 / samples;
				float4 mask = 0;
				for (int j = 0; j < samples; j++) {
					float2 uv = (position + float2(cos(j * angleStep), sin(j * angleStep)) * blurRadius) / _Size;
					mask += MakeTerrainMask(tex2D(_MainTex, uv), _Thresholds);
				}
				mask /= samples;
				return mask;
			}
		ENDCG
		}
		
		Pass
		{
			Name "BushMask"
			
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
			
			sampler2D _MainTex;
			fixed2 _Size;
			float _Radius, _Smoothness, _RadiusNoiseScale, _BlurRadiusNoiseScale;
			float4 _Thresholds;
			float2 _RadiusRange, _BlurRadiusRange;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			#include "Assets/Shaders/ClassicNoise.cginc"
			#include "Assets/Shaders/TerrainMask.cginc"
			
			fixed4 frag(v2f i) : SV_Target
			{
				float2 position = i.uv * _Size;
				
				float4 neighborMaskMX = MakeTerrainMask(tex2D(_MainTex, (position - float2(_Radius, 0)) / _Size), _Thresholds);
                float4 neighborMaskMY = MakeTerrainMask(tex2D(_MainTex, (position - float2(0, _Radius)) / _Size), _Thresholds);
				float4 neighborMaskPX = MakeTerrainMask(tex2D(_MainTex, (position + float2(_Radius, 0)) / _Size), _Thresholds);
				float4 neighborMaskPY = MakeTerrainMask(tex2D(_MainTex, (position + float2(0, _Radius)) / _Size), _Thresholds);
				float result = 1;
				result = min(result, dot(neighborMaskMX, neighborMaskPX));
				result = min(result, dot(neighborMaskMY, neighborMaskPY));
				result = 1-result;
				 
				return float4(result,result,result,1);
			}
		ENDCG
		}
	}
}
