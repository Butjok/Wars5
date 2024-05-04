Shader "Custom/DistortionFlow" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SDF ("_SDF", 2D) = "white" {}
		[NoScaleOffset] _FlowMap ("Flow (RG, A noise)", 2D) = "black" {}
		[NoScaleOffset] _DerivHeightMap ("Deriv (AG) Height (B)", 2D) = "black" {}
		_UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
		_Tiling ("Tiling", Float) = 1
		_Speed ("Speed", Float) = 1
		_Scale ("_Scale", Float) = 1
		_FlowStrength ("Flow Strength", Float) = 1
		_FlowOffset ("Flow Offset", Float) = 0
		_HeightScale ("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated ("Height Scale, Modulated", Float) = 0.75
		_WaterFogColor ("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity ("Water Fog Density", Range(0, 10)) = 0.1
		_RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.25
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Normal ("_Normal", 2D) = "normal" {}
		_Grid ("_Grid", 2D) = "black" {}
		
		_WaveAmplitude ("_WaveAmplitude", Float) = 0.0001
		
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		
		_BackgroundColor ("_BackgroundColor", Color) = (0, 0, 0, 0)
		
		_DistanceField ("Distance Field", 2D) = "white" {}
		_Noise ("Noise", 2D) = "black" {}
		_FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
		
		_TimeMultiplier ("Time Multiplier", Float) = .25
		_WaveFrequency ("Wave Frequency", Float) = 5
		_WaveMaskSharpness ("Wave Mask Sharpness", Float) = 4
		_WaveSmashingPower ("Wave Smashing Power", Float) = .75
		_WaveSharpness1 ("Wave Sharpness 1", Float) = 5
		_WaveSharpness2 ("Wave Sharpness 2", Float) = 3.33
		
		_NormalPower ("Normal Power", Float) = 1
		_OcclusionTint ("Occlusion Tint", Float) = 1
		_GridColor ("Grid Color", Color) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		
		GrabPass { "_WaterBackground" }

		
			
		CGPROGRAM

		
		
		#pragma surface surf Standard alpha finalcolor:ResetAlpha vertex:vert
		#pragma target 3.0
		#pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF

		#include "Flow.cginc"
		#include "LookingThroughWater.cginc"

		sampler2D _MainTex, _FlowMap, _DerivHeightMap,_Normal,_SDF,_Grid;
		float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
		float _HeightScale, _HeightScaleModulated;
		half _Scale;
		float _WaveAmplitude;
		sampler2D _DistanceField;
		sampler2D _Noise;
		float4 _FoamColor;
		float _TimeMultiplier, _WaveFrequency, _WaveMaskSharpness, _WaveSmashingPower;
		float _WaveSharpness1, _WaveSharpness2, _NormalPower;
		float4 _GridColor;
		
		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color, _BackgroundColor;
		float4 _Normal_ST, _Noise_ST;

		float3 UnpackDerivativeHeight (float4 textureData) {
			float3 dh = textureData.agb;
			dh.xy = dh.xy * 2 - 1;
			return dh;
		}

		void vert(inout appdata_full data){
            half distance =tex2Dlod (_SDF,  float4(data.texcoord.xy, 0.0, 0.0)).r;
            half wave =  sin(distance*_Scale + _Speed*_Time.y + data.vertex.x*5 + data.vertex.z*5);
            half wavesMask = smoothstep(.5, .0, distance);
            data.vertex.y += wavesMask*wave*_WaveAmplitude;
                                          
        }
        
        float _OcclusionTint;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 flow = tex2D(_FlowMap, IN.uv_MainTex).rgb;
			flow.xy = flow.xy * 2 - 1;
			flow *= _FlowStrength;
			float noise = tex2D(_FlowMap, IN.uv_MainTex).a;
			float time = _Time.y * _Speed + noise;
			float2 jump = float2(_UJump, _VJump);

			float3 uvwA = FlowUVW(
				IN.uv_MainTex, flow.xy, jump,
				_FlowOffset, _Tiling, time, false
			);
			float3 uvwB = FlowUVW(
				IN.uv_MainTex, flow.xy, jump,
				_FlowOffset, _Tiling, time, true
			);

			float finalHeightScale =
				flow.z * _HeightScaleModulated + _HeightScale;
			

			float3 dhA =
				UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwA.xy)) *
				(uvwA.z * finalHeightScale);
			float3 dhB =
				UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwB.xy)) *
				(uvwB.z * finalHeightScale);
			o.Normal = normalize(float3(-(dhA.xy + dhB.xy), 1));

			fixed4 texA = tex2D(_MainTex, uvwA.xy) * uvwA.z;
			fixed4 texB = tex2D(_MainTex, uvwB.xy) * uvwB.z;

			fixed4 c = (texA + texB) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			

			float2 position = IN.worldPos.xz;
            
            // Albedo comes from a texture tinted by color

			_Time.x *= 1.25;
			
            fixed3 normal3 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5)/6 + float2(_Time.x*.125/3, _Time.x*.125/3)),_Normal)));
            fixed3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5)*2 + float2(_Time.x*3/3, 0)),_Normal)));
            fixed3 normal2 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5) - float2(0, _Time.x*4.676/3)),_Normal)));
			float3 targetNormal =BlendNormals(lerp(float3(0,0,1),normal,1), lerp(float3(0,0,1),normal2,.5));
			//targetNormal = normalize(normal/2 + normal2);
			targetNormal =BlendNormals(lerp(float3(0,0,1),normal3,1), targetNormal);
			o.Normal = normalize( targetNormal);

			o.Normal = targetNormal;

			float4 colorBelowWater = ColorBelowWater(IN.screenPos, o.Normal);
			//if (colorBelowWater.a < 0.1)
			//	colorBelowWater = _BackgroundColor;
			//clip(colorBelowWater.a - 0.5);
			
			//colorBelowWater = lerp(_BackgroundColor,colorBelowWater, colorBelowWater.a);
			
			o.Emission = colorBelowWater * _Color.rgb * (1 - c.a);
			//o.Emission = ColorBelowWater(IN.screenPos, o.Normal).a;;


			float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
			float fresnel = dot(viewDir, float3(0,1,0));
			o.Occlusion=lerp(.25, 1, smoothstep (0.75, .9, fresnel)) * _OcclusionTint;
			//o.Emission = o.Occlusion;


			//o.Emission = o.Normal;

			float noise2 = tex2D(_Noise, TRANSFORM_TEX(IN.worldPos.xz, _Noise) ).r;
			float distance = tex2D (_DistanceField, IN.uv_MainTex).a;
			float waveMask = pow(1 - saturate(distance), _WaveMaskSharpness);
			float fraction = 1 - frac(pow(distance, _WaveSmashingPower) * _WaveFrequency + _Time.y * _TimeMultiplier + noise2 * 2.5	);
			float wave = pow(max(fraction, 1 - saturate(fraction * _WaveSharpness1)), _WaveSharpness2);
			o.Emission += (wave) * waveMask * _FoamColor;

			//o.Emission = noise2;

			//o.Emission = noise2;
			
			o.Normal = sign(o.Normal) * normalize(pow(abs(o.Normal), _NormalPower));

			//o.Emission = tex2D(_DistanceField, IN.uv_MainTex).a;

			//o.Emission = waveMask;
			o.Emission += lerp(0, _GridColor, tex2D (_Grid, position-.5));
		}
		
 		void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			color.a = 1;
			
		}
		
		ENDCG
	}
}