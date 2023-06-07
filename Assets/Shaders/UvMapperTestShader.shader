Shader "Custom/UvMapperTestShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Threshold ("Threshold", Range(0,1)) = 0.05
		_Offset ("_Offset", Range(0,1)) = 0.333
		_Visibility ("_Visibility", 2D) = "white" {}
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
		half _Metallic, _Threshold, _Offset;
		fixed4 _Color;
		
		sampler2D _Visibility;
		float4x4 _Visibility_WorldToLocal;
		
		sampler2D _Bounds;
		float4x4 _Bounds_WorldToLocal;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			
			o.Albedo = 0;
			
			float2 uv = mul(_Visibility_WorldToLocal, float4(IN.worldPos, 1)).xz;
			float distance = tex2D (_Visibility, uv).r;
			if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
				distance = -1;
			
			half visibilityIntensity = (smoothstep(_Offset + _Threshold, _Offset - _Threshold, distance));
			
			o.Albedo.rgb = float3(1,1,1) * visibilityIntensity;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}