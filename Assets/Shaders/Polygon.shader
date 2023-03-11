Shader "Custom/Polygon" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Speed ("_Speed", Float) = 0.0
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
		int _Count;
		float2 _From[128], _To[128];
		float _Speed, _StartTime;

		float easeInOutQuad(float x){
			return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Albedo = 1;
			if (_Count > 0) {
				float2 position = IN.worldPos.xz;
				half minDistance = 9999999;
				for (int i = 0; i < _Count; i++) {

					float distance = length(_To[i] - _From[i]);
					float duration = distance / _Speed;
					float time = clamp(_Time.y - _StartTime, 0, duration);
					float t = time / duration;
					float2 pt = lerp(_From[i], _To[i], easeInOutQuad(t));
					
					float d = length(position - pt);
					if (d < minDistance)
						minDistance = d;
				}
				o.Albedo = minDistance;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}