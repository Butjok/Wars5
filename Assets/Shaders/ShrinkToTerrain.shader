Shader "Custom/ShrinkToTerrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Splat ("_Splat", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _Splat;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float4 _Bounds;
		float4 _Flip;

		void vert(inout appdata_full v) {
		#ifdef SHADER_API_D3D11

		    //const float4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

			float4 worldPos = mul(unity_ObjectToWorld , v.vertex);

			float2 splatUv = (worldPos.xz - _Bounds.xy) / (_Bounds.zw);
            if (_Flip.x > .5)
                splatUv.x = 1 - splatUv.x;
            if (_Flip.y > .5)
                splatUv.y = 1 - splatUv.y;

			half height = tex2Dlod(_Splat, float4(splatUv,0,0));
			worldPos.y = height;

			v.vertex.xyz = mul(unity_WorldToObject, worldPos).xyz;
		    //v.vertex = mul(transform, v.vertex);
		    //v.tangent = mul(transform, v.tangent);
		    //v.normal = normalize(mul(transform, v.normal));

		#endif 
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o) {

float2 splatUv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw);
            if (_Flip.x > .5)
                splatUv.x = 1 - splatUv.x;
            if (_Flip.y > .5)
                splatUv.y = 1 - splatUv.y;

			
			o.Albedo = tex2D(_Splat, splatUv);

			//o.Albedo = 0;
			//o.Albedo.rg = _Flip;
		}
		ENDCG
	}
	FallBack "Diffuse"
}