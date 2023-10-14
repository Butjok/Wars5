Shader "Unlit/FrostedGlass3D"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_AdditiveColor ("Additive Color", Color) = (0,0,0,0)
		_Radius ("Radius", Float) = 150
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 100

		GrabPass { "_GrabTexture" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
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
				float4 grabUv : TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex, _GrabTexture;
			float4 _MainTex_ST;
			float4 _GrabTexture_TexelSize;
			float3 _TintColor, _AdditiveColor;
			float _Radius;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float N21(float2 p) {
				float2 p2 = frac(float2(p) * float2(443.897, 441.423));
				p2 += dot(p2, p2.yx + 19.19);
				return frac((p2.xx + p2.yx)*p2.yy);
			}
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float samples = 32;
				float3 sum = 0;
				float2 projUv = i.grabUv.xy / i.grabUv.w;
				float a = N21(i.grabUv.xy) * 6.28318530718;
				for (float j = 0; j < samples; j++) {
					float2 offs = float2(sin(a), cos(a)) * _Radius;
					float d = frac(sin((a+j+1) * 12.9898 + 78.233) * 43758.5453);
					d = sqrt(d);
					offs *= d;
					sum += tex2D(_GrabTexture, projUv + offs * _GrabTexture_TexelSize.xy).rgb;
					a++;
				}
				return float4(sum / samples * _TintColor + _AdditiveColor, 1);
				
				
			}
			ENDCG
		}
	}
}