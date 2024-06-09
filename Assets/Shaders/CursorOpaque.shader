Shader "Custom/CursorOpaque" {
	Properties {
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_TerrainHeight ("Terrain Height", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "LightMode" = "Deferred" }
		LOD 200
		
		ZWrite Off
		ZTest Always
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _TerrainHeight;
		float4  _Tint;
		fixed4x4 _WorldToTerrainHeightUv;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float2 terrainHeightUv = mul(_WorldToTerrainHeightUv, float4(worldPos, 1)).xz;
			float terrainHeight = tex2Dlod(_TerrainHeight, float4(terrainHeightUv, 0, 0)).r;
			v.vertex *= 1 + abs(frac(_Time.y*2)-0.5)*.1;
			v.vertex.z -= terrainHeight;
			//v.vertex += float4(0, 2, 0, 0);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = 0;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			clip (c.a - 0.5);

			o.Emission = _Color.rgb;
		}
		ENDCG
	}
}