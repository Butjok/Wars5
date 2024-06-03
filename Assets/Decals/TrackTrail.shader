Shader "Custom/TrackTrail" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "LightMode" = "Deferred" "Queue"="Geometry+100" }
		Cull off
		ZWrite off
		//ZTest Always
		LOD 200
		
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;
		float _Length;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			// Metallic and smoothness come from slider variables
			o.Metallic = 0;
			o.Smoothness = 0;
			float position = _Length * (1-IN.uv_MainTex.x);
			float alpha = smoothstep(5, 2.5, position);
			o.Albedo = _Color.rgb;
			o.Alpha = alpha * _Color.a;
		}
		ENDCG
	}
}