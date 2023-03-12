Shader "Custom/Polygon" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Speed ("_Speed", Float) = 0.0
		_BorderColor ("_BorderColor", Color) = (1,1,1,1)
		_FillColor ("_FillColor", Color) = (1,1,1,1)
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
		fixed4 _Color, _BorderColor, _FillColor;
		int _Count;
		float4 _From[128], _To[128];
		float _Speed, _StartTime;

		float easeInOutQuad(float x){
			return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
		}
		float easeOutSine(float x){
			return sin((x * 3.141592) / 2);
		}

		float2 V(int i, float t) {
			return lerp(_From[i], _To[i], t);
		}

		float sdPolygon( float2 p, float t )
		{
			float v0 = V(0,t);
			float d = dot(p-v0,p-v0);
			float s = 1.0;
			for( int i=0, j=_Count-1; i<_Count; j=i, i++ )
			{
				// distance
				float2 vj = V(j,t);
				float2 vi = V(i,t);
				float2 e = vj - vi;
				float2 w =    p - vi;
				float2 b = w - e*clamp( dot(w,e)/dot(e,e), 0.0, 1.0 );
				d = min( d, dot(b,b) );
		
				// winding number from http://geomalgorithms.com/a03-_inclusion.html
				bool3 cond = bool3( p.y>=vi.y,
									p.y <vj.y,
									e.x*w.y>e.y*w.x );
				
				if( all(cond) || all(!cond) ) s=-s;
			}
		
			return s*sqrt(d);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Albedo = _Color;;
			if (_Count > 0) {
				float2 position = IN.worldPos.xz;
				float time = clamp(_Time.y - _StartTime, 0, _Speed);
				float t = time / _Speed;
				t = easeOutSine(t);
				float distance = sdPolygon(position, t);
				float fill = smoothstep(.025, -.025, distance - .05);
				float border = smoothstep(.0075 + .005, .005, abs(distance - .05));
				o.Emission = _BorderColor*border + _FillColor*fill;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}