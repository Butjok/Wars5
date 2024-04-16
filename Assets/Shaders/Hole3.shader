Shader "Custom/Hole3" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Radius ("_Radius", Float) = .5
		_HoleMask ("Hole Mask", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _HoleMask;
		fixed4x4 _HoleMask_WorldToLocal;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 objectWorldPosition;
			float hole;
		};
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.objectWorldPosition = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
			o.hole = tex2Dlod(_HoleMask, float4(mul(_HoleMask_WorldToLocal, float4(o.objectWorldPosition,1)).xz, 0, 0)).r;
		}

		half _Glossiness;
		half _Metallic;
		half _Radius;
		fixed4 _Color;
		
		float3 RayPlaneIntersection(float3 origin, float3 direction, float3 pointOnPlane, float3 planeNormal)
		{
			float epsilon = 1e-6; // Small value for tolerance
			
			// Calculate the dot product of the ray direction and the plane normal
			float denom = dot(direction, planeNormal);
			
			// Check if the ray and plane are parallel or nearly parallel
			if (abs(denom) < epsilon)
			{
				// Ray and plane are parallel, return a point at infinity
				return float3(999, 999, 999);
			}
			
			// Calculate the distance from the ray origin to the plane
			float3 rayToPlane = pointOnPlane - origin;
			float t = dot(rayToPlane, planeNormal) / denom;
			
			// Calculate the intersection point
			float3 intersectionPoint = origin + t * direction;
			
			return intersectionPoint;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			//o.Albedo = IN.hole;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			if (IN.hole > .5) {
				float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
				float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, IN.objectWorldPosition, direction);
				
				float distance = length(projectedPoint - IN.objectWorldPosition) - _Radius;
				clip(distance);
			}
			
			//o.Emission = smoothstep(0.033, 0.025, distance) * float3(1,.5,0) * 2;
		}
		
		ENDCG
	}
	FallBack "Diffuse"
}