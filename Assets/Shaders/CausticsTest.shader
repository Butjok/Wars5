Shader "Unlit/CausticsTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Columns ("_Columns", Integer) = 1
		_Rows ("_Rows", Integer) = 1
		_Speed ("_Speed", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			int _Columns;
			int _Rows;
			float _Speed;
			
			fixed4 frag (v2f i) : SV_Target
			{
				int length = _Columns * _Rows;
				int frame = round(_Speed * _Time.y) % length;
				int column = frame % _Columns;
				int row = frame / _Columns;
				float2 offset = float2(1.0 / _Columns, 1.0 / _Rows);
				float2 uv = i.uv * offset;
				uv += offset * float2(column, _Rows-row-1);
				
				fixed4 col = tex2D(_MainTex, uv);
				return col;
			}
			ENDCG
		}
	}
}