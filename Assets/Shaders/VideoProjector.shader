Shader "Custom/VideoProjector" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
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
		fixed4x4 _WorldToProjection;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
			o.Albedo = _Color;
			
			half3 video = 0;
			float2 projectionSpace = (mul(_WorldToProjection, float4(IN.worldPos, 1)).xy + 1) / 2;
			if (projectionSpace.x >= 0 && projectionSpace.x <= 1 && projectionSpace.y >= 0 && projectionSpace.y <= 1)
				video = tex2D(_MainTex, projectionSpace).rgb;
				
			o.Emission = video * o.Albedo.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}