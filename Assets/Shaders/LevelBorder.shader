Shader "Unlit/LevelBorder"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (1,1,1,1)
		_Power ("Power", Range(0.1, 10)) = 1
		_LineColor ("LineColor", Color) = (1,1,1,1)
		_CellCount ("CellCount", Range(1, 100)) = 10
	}
	SubShader
	{
		Tags {  "Queue"="Transparent"  "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma alpha:fade
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Power;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// set uv to the screen uv (0,0) to (1,1)
				o.uv = o.vertex.xy;
				return o;
			}

			float4 _Color;
			float4 _Color2;
			float4 _LineColor;
			float _CellCount;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// get the pixel uv on the screen
				//
				// 0,0  1,0

				//return _Color;

				float ratio = _ScreenParams.x / _ScreenParams.y;
				float2 uv2 = i.vertex.xy /  _ScreenParams.xy;
				uv2.x *= ratio;
				float2 cell = frac(uv2*_CellCount);
				float2 a = smoothstep(.05,.075,abs(cell-.5));
				float lin = 1-min(a.x,a.y);

				float2 uv = i.vertex.xy /  _ScreenParams.xy;
				uv -= .5;
				uv = pow (abs(uv), _Power);
				float dist = smoothstep(0.0, .5, length(uv));
				float4 c = 1;
				c.rgb = lerp(_Color, _Color2, dist).rgb;
				 c.rgb += lin * _LineColor.rgb;
				return c;				
			}
			ENDCG
		}
	}
}