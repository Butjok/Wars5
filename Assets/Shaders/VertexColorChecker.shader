Shader "Custom/VertexColorChecker" {
	Properties {
		_ColorA ("_ColorA", Color) = (1,1,1,1)
		_ColorB ("_ColorB", Color) = (0,0,0,1)
		_Tint ("_Tint", Color) = (1,1,1,1)
		_Scale ("_Scale", Vector) = (1,1,0,0)
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

		#include "Utils.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _ColorA, _ColorB, _Tint;
		half2 _Scale;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			int x = round(IN.worldPos.x);
			int y = round(IN.worldPos.z);
			o.Albedo = Tint(IN.color.rgb, 0, 1.5, .5);
			if ((x+y) % 2 == 0) {
				o.Albedo = Tint(o.Albedo, 0, 1.1, .85);
			}
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = IN.color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}