Shader "Custom/Lily" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_ReUv ("_ReUv", 2D) = "black" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Noise ("Noise", 2D) = "black" {}
		_Threshold ("Threshold", Range(0,1)) = 0.5
		_UseColor ("Use Color", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _ReUv, _Noise;
		float4 _Noise_ST;
		float _UseColor;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Threshold;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
		
		#include "Assets/Shaders/Utils.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			float2 actualUv = floor(IN.uv_MainTex) + tex2D(_ReUv, IN.uv_MainTex).rg;

			actualUv.y += sin (actualUv.x * 10) * 0.1; 
			
			float noise = tex2Dlod(_Noise, float4(TRANSFORM_TEX(actualUv, _Noise),0,0)).r;
			float on = step(_Threshold, noise);
			c.a *= on;
			
			clip(c.a - .5);
			
			o.Albedo = lerp(c.rgb, _Color, _UseColor);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			float3 hsv = RGBtoHSV(o.Albedo.rgb);
			hsv.z *= 1.5;
			o.Albedo.rgb = HSVtoRGB(hsv);

			//o.Albedo=0;
			//o.Albedo.rg = actualUv;
		}
		ENDCG
	}
	FallBack "Diffuse"
}