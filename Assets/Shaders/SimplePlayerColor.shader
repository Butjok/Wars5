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
		#pragma surface surf Standard vertex:vert addshadow 
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
		fixed4 _Color;
		
		#if HOLE	
		half _HoleRadius;
		sampler2D _HoleMask;
		float4x4 _HoleMask_WorldToLocal;
		#endif

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float3, _PlayerColor)
			UNITY_DEFINE_INSTANCED_PROP(float, _PlayerColorIntensity)
		UNITY_INSTANCING_BUFFER_END(Props)
		
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

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb *  lerp(float3(1,1,1), UNITY_ACCESS_INSTANCED_PROP(Props, _PlayerColor).rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _PlayerColorIntensity));
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			#if HOLE
			if (IN.hole.a > 0.5) {
				float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
				float3 holePosition = IN.objectWorldPosition;
				holePosition += float3(0,0.25,0);
				float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, holePosition, direction);
				
				float distance = length(projectedPoint - holePosition) - _HoleRadius;
				clip(distance);

				float circleBorder = smoothstep(0.025, 0.015, distance);
				o.Emission = circleBorder * IN.hole.rgb*2;
				o.Albedo *= 1 - circleBorder;
			}
			#endif	
		
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}