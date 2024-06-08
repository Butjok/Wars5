Shader "Unlit/VertexColor"
{
	Properties
	{
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "black" {}
		_ShowAlpha ("Show Alpha", Float) = 0
		_TerrainHeight ("Terrain Height", 2D) = "black" {}  
		[HDR] _BorderColor ("Border Color", Color) = (1,1,1,1)
		[HDR] _InsideColor ("Inside Color", Color) = (1,1,1,1)
		[HDR] _CircleColor ("Circle Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" "LightMode" = "Deferred" }
		LOD 100
		
		ZWrite Off
		ZTest Always
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 color : COLOR;
			float3 worldPos;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		sampler2D _TerrainHeight;
        float4x4 _WorldToTerrainHeightUv;
		float3 _InsideColor;
		float3 _BorderColor;
		float3 _CircleColor;

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float height = tex2Dlod(_TerrainHeight, float4(mul(_WorldToTerrainHeightUv, float4(worldPos, 1)).xz, 0, 0)).r;
			v.vertex.y += height;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			o.Albedo = 0;
			o.Metallic = 0;
			o.Smoothness = 0;

			float border = smoothstep(.08,0,abs(IN.color.r - .9));
			float inside = IN.color.r < .9;

			float a = 0;
			a = inside *.1;
			a = lerp(a, 1, border);
			o.Alpha = _Color.a * a;

			int2 tilePosition = round(IN.worldPos.xz);
			float distanceToTile = length(IN.worldPos.xz - tilePosition);
			float circle = smoothstep(.06,.04, distanceToTile);
			o.Alpha += circle;

			int2 tile = floor(IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy / 1);
			o.Alpha = border + circle + inside * (1 - saturate((tile.x+tile.y) % 2));
			
			//o.Alpha += frac((IN.worldPos.x + IN.worldPos.z)*5)*inside;

			clip(o.Alpha - .5);

			o.Emission = inside * _InsideColor;
			o.Emission = lerp(o.Emission, _BorderColor, border);
			o.Emission = lerp(o.Emission, _CircleColor, circle);

			//o.Albedo = 0;
		}
		ENDCG
	}
}