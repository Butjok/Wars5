Shader "Custom/HeightMapOffset" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_TerrainHeight ("Terrain Height", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		sampler2D _TerrainHeight;
        float4x4 _WorldToTerrainHeightUv;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		#pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _PlayerColor)
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float2 terrainHeightUv = mul(_WorldToTerrainHeightUv, float4(worldPos, 1)).xz;
			float terrainHeight = tex2Dlod(_TerrainHeight, float4(terrainHeightUv, 0, 0)).r;
			v.vertex.y += terrainHeight;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float4 playerColor = UNITY_ACCESS_INSTANCED_PROP(Props, _PlayerColor);
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Albedo *= lerp(float3(1,1,1), playerColor.rgb, playerColor.a);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
}