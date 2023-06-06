Shader "Custom/Hole2" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Radius ("_Radius", Float) = .5
		_Origin ("_Origin", Vector) = (0,0,0,0)
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
		float _Radius;
		float3 _Origin;

		float CylinderSDF(float3 q, float3 p, float3 d, float r) {
			d = normalize(d);
			q -= p;
			return length(q - (dot(q,d)*d)) - r;
		}
		float3 ProjectPointOntoPlane(float3 P, float3 O, float3 N)
                {
                    float3 V = P - O;
                    float d = dot(V, N);
                    float3 projectedPoint = P - d * N;
                    
                    return projectedPoint;
                }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			float3 origin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz + _Origin.xyz;	
			float3 direction = IN.worldPos - _WorldSpaceCameraPos;
			float3 projectedPoint = ProjectPointOntoPlane(IN.worldPos, origin, direction);
			
			float distance = CylinderSDF(projectedPoint, origin, direction, _Radius);
			clip(CylinderSDF(projectedPoint, origin, direction, _Radius));
			
			o.Emission = smoothstep(0.033, 0.025, distance) * float3(1,.5,0) * 2;
			
			o.Albedo = c.rgb;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}