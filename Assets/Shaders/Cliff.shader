Shader "Custom/Cliff" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_GrassMask ("Grass Mask", 2D) = "white" {}
		_Normal ("Normal Map", 2D) = "bump" {}
		_CliffHSVTweak ("_CliffHSVTweak", Vector) = (.025, 1.1, 1)
		_GrassHSVTweak ("_GrassHSVTweak", Vector) = (-.01, 1.25, 1.25)
		_SeaLevel ("Sea Level", Float) = 0
		_SeaSharpness ("Sea Sharpness", Float) = .1
		_SeaColor ("Sea Color", Color) = (0, 0, 1, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _GrassMask;
		sampler2D _Normal;

		#include "Assets/Shaders/Utils.cginc"

		 float3 _GrassHSVTweak;
		 float3 _CliffHSVTweak;

		float _SeaLevel;
		float _SeaSharpness;
		float3 _SeaColor;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			half grass = tex2D(_GrassMask, IN.uv_MainTex).r;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
			o.Alpha = c.a;

			float3 grassHSV = RGBtoHSV(c.rgb);
			grassHSV.x += _GrassHSVTweak.x;
			grassHSV.y *= _GrassHSVTweak.y;
			grassHSV.z *= _GrassHSVTweak.z;
			o.Albedo = lerp(c.rgb, HSVtoRGB(grassHSV), grass);
			float3 cliffHSV = RGBtoHSV(c.rgb);
			cliffHSV.x += _CliffHSVTweak.x;
			cliffHSV.y *= _CliffHSVTweak.y;
			cliffHSV.z *= _CliffHSVTweak.z;
			o.Albedo = lerp(o.Albedo, HSVtoRGB(cliffHSV), 1 - grass);

			float sea = smoothstep(_SeaLevel - _SeaSharpness, _SeaLevel + _SeaSharpness, IN.worldPos.y);
			o.Albedo = lerp(o.Albedo, _SeaColor, sea);
		}
		ENDCG
	}
	FallBack "Diffuse"
}