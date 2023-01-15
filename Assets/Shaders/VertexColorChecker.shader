Shader "Custom/VertexColorChecker" {
	Properties {
		_ColorA ("_ColorA", Color) = (1,1,1,1)
		_ColorB ("_ColorB", Color) = (0,0,0,1)
		_Tint ("_Tint", Color) = (1,1,1,1)
		_Scale ("_Scale", Vector) = (1,1,0,0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_ColorPlain ("_ColorPlain", Color) = (1,1,1,1)
		_ColorRoad ("_ColorRoad", Color) = (1,1,1,1)
		_ColorSea ("_ColorSea", Color) = (1,1,1,1)
		_ColorMountain ("_ColorMountain", Color) = (1,1,1,1)
	
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
		fixed4 _ColorPlain, _ColorRoad, _ColorSea, _ColorMountain;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			int x = round(IN.worldPos.x);
			int y = round(IN.worldPos.z);
			
			float epsilon = .01;
			o.Smoothness = 0;
			
			float type = IN.color.a;
			if (abs(type - 1) < epsilon) { 
				o.Albedo = _ColorPlain; 
			}
			else if (abs(type - 2) < epsilon) {
				o.Albedo = _ColorRoad;
			}
			else if (abs(type - 4) < epsilon) {
				o.Albedo = _ColorSea;
				o.Smoothness = 1;
			}
			else if (abs(type - 8) < epsilon) {
				o.Albedo = _ColorMountain;
			}
			else 
			 o.Albedo = IN.color.rgb;
			
			if ((x+y) % 2 == 0) {
				o.Albedo = Tint(o.Albedo, 0, 1.00, .90);
			}
			// Metallic and smoothness come from slider variables
			o.Metallic = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}