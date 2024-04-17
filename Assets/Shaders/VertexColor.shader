Shader "Unlit/VertexColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShowAlpha ("Show Alpha", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

// blend in multiply mode
			Blend SrcAlpha OneMinusSrcAlpha
		// dont use Z buffer
			ZWrite Off
			//ZTest Always

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
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float3 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    half inside = smoothstep(1,0,1-i.color.a);
			    half border = smoothstep(.5,0,abs(1-i.color.a - 0.5));

				float2 cell = round(i.worldPos.xz);
        		float2 distanceToCell = length(cell - i.worldPos.xz);
        		float circle = smoothstep(0.05, 0.025, distanceToCell);
				
			    return (inside/2 + border) + inside * circle;
			}
			ENDCG
		}
	}
}