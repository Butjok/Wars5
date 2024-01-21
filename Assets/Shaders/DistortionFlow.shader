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
		_WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
		_RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.25
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Normal ("_Normal", 2D) = "normal" {}
		_Grid ("_Grid", 2D) = "black" {}
		
		_WaveAmplitude ("_WaveAmplitude", Float) = 0.0001
		
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
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
		
		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _Normal_ST;

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
            fixed3 normal3 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5)/12 + float2(_Time.x*.125/3, _Time.x*.125/3)),_Normal)));
            fixed3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5)*2 + float2(_Time.x*3/3, 0)),_Normal)));
            fixed3 normal2 = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(((position/1.5) - float2(0, _Time.x*4.676/3)),_Normal)));
			float3 targetNormal =BlendNormals(lerp(float3(0,0,1),normal,.5), lerp(float3(0,0,1),normal2,.25));
			//targetNormal = normalize(normal/2 + normal2);
			targetNormal =BlendNormals(lerp(float3(0,0,1),normal3,.75), targetNormal);
			o.Normal = normalize( targetNormal);

			o.Emission = ColorBelowWater(IN.screenPos, o.Normal) * _Color.rgb * (1 - c.a);
			o.Occlusion=0.25;

			o.Albedo *= 1-tex2D (_Grid, position-.5);
		}
		
 		void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			color.a = 1;
		}
		
		ENDCG
	}
}