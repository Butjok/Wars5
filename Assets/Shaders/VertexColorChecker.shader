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
		_ColorForest ("_ColorForest", Color) = (1,1,1,1)
		_ColorRiver ("_ColorRiver", Color) = (1,1,1,1)

		_Emissive ("_Emissive", Color) = (0,0,0,1)

		_Road ("_Road", 2D) = "white" {}
	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "Utils.cginc"

		sampler2D _MainTex,_Road;

		struct Input {
//			float2 uv_Road;
			float3 worldPos;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _ColorA, _ColorB, _Tint;
		half2 _Scale;
		fixed4 _ColorPlain, _ColorRoad, _ColorSea, _ColorMountain,_ColorForest, _ColorRiver,_Emissive;

		void vert (inout appdata_full v) {
			float type = v.color.a;
			if (abs(type - 4) < .01) {
//				v.vertex.y -= 0.1;
			}
			else if (abs(type - 8) < .01) {
				//v.vertex.y += 0.1;
			}
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			int x = round(IN.worldPos.x);
			int y = round(IN.worldPos.z);
			
			float epsilon = .01;
			o.Smoothness = 0;
			
			float4 color = float4(0,0,0,0);
			
			float type = IN.color.a;
			if (abs(type - 1) < epsilon) {
				color = _ColorPlain; 
			}
			else if (abs(type - 2) < epsilon) {
				color = tex2D(_Road, IN.worldPos.xz - float2(x,y) + .5);
				color *= _ColorRoad;
			}
			else if (abs(type - 4) < epsilon) {
				color = _ColorSea;
			}
			else if (abs(type - 8) < epsilon) {
				color = _ColorMountain;
			}
			else if (abs(type - 16) < epsilon) {
				color = _ColorForest;
			}
			else if (abs(type - 32) < epsilon) {
				color = _ColorRiver;
			}
			else {
				color = _ColorPlain;
			}
			
			
			if ((x+y) % 2 == 0) {
				color.rgb = Tint(color.rgb, 0, 1.00, .925);
			}
			
			half2 distances = abs(IN.worldPos.xz - float2(x,y));
			half2 distance = max(distances.x, distances.y);
			half border = smoothstep(.475, .5, distance);
			
			// Metallic and smoothness come from slider variables
			o.Albedo = color.rgb;
			o.Albedo *= lerp(1,(1-border),.25);
			o.Metallic = 0;
			o.Emission = _Emissive;
			o.Smoothness = color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}