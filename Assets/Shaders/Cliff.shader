Shader "Custom/Cliff" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_GrassMask ("Grass Mask", 2D) = "white" {}
		_Normal ("Normal Map", 2D) = "bump" {}
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
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _GrassMask;
		sampler2D _Normal;

		#include "Assets/Shaders/Utils.cginc"

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

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
			grassHSV.x -= .01f;;
			grassHSV.y *= 1.25;
			grassHSV.z *= 1.25;
			o.Albedo = lerp(c.rgb, HSVtoRGB(grassHSV), grass);
			float3 cliffHSV = RGBtoHSV(c.rgb);
			cliffHSV.x += .025f;
			cliffHSV.y *= 1.1;
			//cliffHSV.z *= 1.125;
			o.Albedo = lerp(o.Albedo, HSVtoRGB(cliffHSV), 1 - grass);
		}
		ENDCG
	}
	FallBack "Diffuse"
}