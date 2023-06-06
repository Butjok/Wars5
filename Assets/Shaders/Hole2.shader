Shader "Custom/Hole2" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Radius ("_Radius", Float) = .5
		_Origin ("_Origin", Vector) = (0,0,0,0)
		_UnitPosition ("_UnitPosition", Vector) = (0,0,0,0)
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
		float3 _UnitPosition;

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
                
                bool RayBoxIntersection(float3 origin, float3 direction, float3 boxOrigin, float3 boxHalfSize)
                {
                    float3 tMin = (boxOrigin - origin) / direction;
                    float3 tMax = (boxOrigin + boxHalfSize - origin) / direction;
                    
                    float3 tEntry = min(tMin, tMax);
                    float3 tExit = max(tMin, tMax);
                    
                    float tEntryMax = max(max(tEntry.x, tEntry.y), tEntry.z);
                    float tExitMin = min(min(tExit.x, tExit.y), tExit.z);
                    
                    return tEntryMax <= tExitMin;
                }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			//_WorldSpaceCameraPos
			float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
			float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, _UnitPosition, direction);
			
			float distance = length(projectedPoint - _UnitPosition) - _Radius;
			clip(distance);
			
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