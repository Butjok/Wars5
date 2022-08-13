Shader "Custom/DirectionalFlow" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Deriv (AG) Height (B)", 2D) = "black" {}
		[NoScaleOffset] _FlowMap ("Flow (RG)", 2D) = "black" {}
		[Toggle(_DUAL_GRID)] _DualGrid ("Dual Grid", Int) = 0
		_Tiling ("Tiling, Constant", Float) = 1
		_TilingModulated ("Tiling, Modulated", Float) = 1
		_GridResolution ("Grid Resolution", Float) = 10
		_Speed ("Speed", Float) = 1
		_FlowStrength ("Flow Strength", Float) = 1
		_HeightScale ("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated ("Height Scale, Modulated", Float) = 0.75
		_WaterFogColor ("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
		_RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.25
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Normal ("_Normal", 2D) = "normal" {}
		_Scale ("_Scale", Float) = 1
		_SDF ("_SDF", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		GrabPass { "_WaterBackground" }

		CGPROGRAM
		#pragma surface surf Standard alpha finalcolor:ResetAlpha
		#pragma target 3.0

		#pragma shader_feature _DUAL_GRID
		
		#include "Flow.cginc"
		#include "LookingThroughWater.cginc"

		sampler2D _MainTex, _FlowMap,_Normal,_SDF;
		float _Tiling, _TilingModulated, _GridResolution, _Speed, _FlowStrength;
		float _HeightScale, _HeightScaleModulated;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _Normal_ST;
		half _Scale;

		float3 UnpackDerivativeHeight (float4 textureData) {
			float3 dh = textureData.agb;
			dh.xy = dh.xy * 2 - 1;
			return dh;
		}

		float3 FlowCell (float2 uv, float2 offset, float time, float gridB) {
			float2 shift = 1 - offset;
		    shift *= 0;
			offset *= 0.5;
			if (gridB) {
		        offset += 0.25;
		        shift -= 0.25;
		    }
			float2x2 derivRotation;
		    float2 uvTiled =
		        (floor(uv * _GridResolution + offset) + shift) / _GridResolution;
			float3 flow = tex2D(_FlowMap, uvTiled).rgb;
			flow.xy = flow.xy * 2 - 1;
			flow.z *= _FlowStrength;
			float tiling = flow.z * _TilingModulated + _Tiling;
			float2 uvFlow = DirectionalFlowUV(
				uv + offset, flow, tiling, time,
				derivRotation
			);
			float3 dh = UnpackDerivativeHeight(tex2D(_MainTex, uvFlow));
			dh.xy = mul(derivRotation, dh.xy);
			dh *= flow.z * _HeightScaleModulated + _HeightScale;
			return dh;
		}

		float3 FlowGrid (float2 uv, float time, bool gridB) {
		    float3 dhA = FlowCell(uv, float2(0, 0), time, gridB);
			float3 dhB = FlowCell(uv, float2(1, 0), time, gridB);
			float3 dhC = FlowCell(uv, float2(0, 1), time, gridB);
			float3 dhD = FlowCell(uv, float2(1, 1), time, gridB);

			float2 t = uv * _GridResolution;
			if (gridB) {
			    t += 0.25;
			}
			t = abs(2 * frac(t) - 1);
			float wA = (1 - t.x) * (1 - t.y);
			float wB = t.x * (1 - t.y);
			float wC = (1 - t.x) * t.y;
			float wD = t.x * t.y;

			return dhA * wA + dhB * wB + dhC * wC + dhD * wD;
		}

		void vert(inout appdata_full data){
            half distance =tex2Dlod (_SDF,  float4(data.texcoord.xy, 0.0, 0.0)).r;
            half wave = min ( sin(distance*_Scale + _Speed*_Time.y/* + data.vertex.x*5 + data.vertex.z*5*/),
                              sin(distance*_Scale + _Speed*_Time.y/* + data.vertex.x*5 + data.vertex.z*5*/ + 3.1415) );
            half wavesMask = smoothstep(.5, .0, distance);
            data.vertex.y += wavesMask*wave*.0025;
                                          
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float time = _Time.y * _Speed;
			float2 uv = IN.uv_MainTex;
			float3 dh = FlowGrid(uv, time, false);
			#if defined(_DUAL_GRID)
				dh = (dh + FlowGrid(uv, time, true)) * 0.5;
			#endif
			fixed4 c = dh.z * dh.z * _Color;
			c.a = _Color.a;
			o.Albedo = c.rgb;
			o.Normal = normalize(float3(-dh.xy, 1));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			o.Emission = ColorBelowWater(IN.screenPos, o.Normal) * (1 - c.a);

			float2 position = IN.worldPos.xz;
            
            // Albedo comes from a texture tinted by color
            fixed3 normal3 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position/10 + float2(_Time.x*.25, _Time.x*.25)),_Normal)));
            fixed3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position + float2(_Time.x*4, 0)),_Normal)));
            fixed3 normal2 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX((position - float2(0, _Time.x*7)),_Normal)));
			float3 targetNormal = normal + normal2 + normal3;
			o.Normal = normalize( targetNormal);
		}
		
		void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			color.a = 1;
		}
		
		ENDCG
	}
}