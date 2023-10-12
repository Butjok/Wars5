Shader "Custom/TileMasked" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MetallicSmoothness ("_MetallicSmoothness", 2D) = "black" {}
		_Normal ("Normal Map", 2D) = "bump" {}
		[HDR]_Emissive ("_Emissive", Color) = (1,1,1,1)	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _MetallicSmoothness;
		sampler2D _TileMask, _Normal;
		fixed4x4 _TileMask_WorldToLocal;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		fixed4 _Color, _Emissive;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 ms = tex2D (_MetallicSmoothness, IN.uv_MainTex);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = ms.r;
			o.Smoothness = ms.a;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
			
			float2 uv2 = mul(_TileMask_WorldToLocal, round(float4(IN.worldPos.xyz, 1))).xz;
                            			float tileMask = saturate(tex2D(_TileMask, uv2).r);
                            			if (uv2.x < 0 || uv2.x > 1 || uv2.y < 0 || uv2.y > 1)
                            				tileMask = 0;
                            			o.Emission += _Emissive * tileMask;
		}
		ENDCG
	}
	FallBack "Diffuse"
}