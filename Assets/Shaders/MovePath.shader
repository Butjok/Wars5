Shader "Custom/MovePath" {
	Properties {
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_UVs ("_UVs", 2D) = "black" {}
		_Alpha ("_Alpha", 2D) = "white" {}
		_SlideTexture ("_SlideTexture", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	
	
	
	
	
	SubShader {
		Tags{ "RenderType" = "Transparent" }
		LOD 200
		ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard   alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _UVs,_Alpha,_SlideTexture;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed2 remappedUvs = tex2D (_UVs, IN.uv_MainTex).rg ;
			clip(tex2D (_Alpha, IN.uv_MainTex).r - .5);
			o.Albedo = _Color;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			//o.Emission = tex2D(_SlideTexture, remappedUvs);
			o.Alpha = tex2D (_Alpha, IN.uv_MainTex).r * _Color.a;
		}
		ENDCG
	}
	
	
	Fallback "Diffuse"
}