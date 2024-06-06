Shader "Custom/ExplosionCrater" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
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
		float _CreationTime;
		float _LifeTime;

		struct Input {
			float2 uv_MainTex;
		};
		
		fixed4 _Color;

		float InverseLerp(float from, float to, float value){
            return (value - from) / (to - from);
        }
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = _Color.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = 0;
			o.Smoothness = 0;
			//o.Albedo = 0;
			float len = distance(IN.uv_MainTex, .5);
			float angle = atan2(IN.uv_MainTex.y - .5, IN.uv_MainTex.x - .5);
			float sin1 = (sin(angle*6) + 1)/2;
			float sin2 = (sin(angle*10) + 1)/2;
			o.Alpha = smoothstep(lerp(.5, .35, .66*sin1+.33*sin2), .0, len) * _Color.a;

			float timeElapsed = _Time.y - _CreationTime;
			o.Alpha *= saturate(InverseLerp(_LifeTime, 0, timeElapsed));
		}
		ENDCG
	}
}