Shader "Custom/FadedEmissive" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Emissive ("_Emissive", 2D) = "black" {}
		_Occlusion ("_Occlusion", 2D) = "white" {}
		_FadeStart ("_FadeStart", Float) = 100
		_FadeEnd ("_FadeEnd", Float) = 25
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _Emissive, _Occlusion;

		struct Input {
			float2 uv_MainTex;
			float distance;
		};

		half _Glossiness;
		half _Metallic;
		half _FadeStart, _FadeEnd;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
		
		void vert (inout appdata_full v, out Input o) {
		    UNITY_INITIALIZE_OUTPUT(Input,o);
		    o.distance = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
	    }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			float emissionIntensity = saturate((IN.distance - _FadeStart) / (_FadeEnd - _FadeStart));
			o.Emission = emissionIntensity * tex2D (_Emissive, IN.uv_MainTex);
			
			o.Occlusion = tex2D (_Occlusion, IN.uv_MainTex);
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}