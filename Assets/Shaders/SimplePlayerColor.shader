Shader "Custom/SimplePlayerColor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_PlayerColor ("Player Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[Toggle(HOLE)] _Hole ("_Hole", Float) = 0
		_HoleRadius ("_HoleRadius", Float) = 0.5
		_PlayerColorIntensity ("Player Color Intensity", Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert 
		#pragma shader_feature HOLE

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			#if HOLE
			float3 worldPos;
			float3 objectWorldPosition;
			float4 hole;
			#endif
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color, _PlayerColor;
		half _PlayerColorIntensity;		
		
		#if HOLE	
		half _HoleRadius;
		sampler2D _HoleMask;
		float4x4 _HoleMask_WorldToLocal;
		#endif
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			#if HOLE
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.objectWorldPosition = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
			o.hole = tex2Dlod(_HoleMask, float4(mul(_HoleMask_WorldToLocal, float4(o.objectWorldPosition,1)).xz, 0, 0));
			#endif
		}
		
		#include "Assets/Shaders/Utils.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			#if HOLE
			if (IN.hole.a > 0.5) {
				float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
				float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, IN.objectWorldPosition, direction);
				
				float distance = length(projectedPoint - IN.objectWorldPosition) - _HoleRadius;
				clip(distance);
				
				o.Emission = smoothstep(0.06, 0.05, distance) * IN.hole.rgb*2;
			}
			#endif	
		
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb *  lerp(float3(1,1,1), _PlayerColor.rgb, _PlayerColorIntensity);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}