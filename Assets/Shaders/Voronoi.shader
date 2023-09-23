Shader "Custom/Voronoi"
{
	Properties
	{
		_Size ("_Size", Vector) = (1,1,1,1)
		_Scale ("_Scale", Float) = 1
		_Power ("_Power", Float) = 1
		_Radius ("_Radius", Float) = 0.01
		_MainTex ("_MainTex", 2D) = "white" {}
		
		_Thresholds ("_Thresholds", Vector) = (.25, 5, .75, 1)
		_Smoothness ("_Smoothness", Float) = 0.05
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			Name "Blur"
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
			fixed2 _Size;
			float _Radius, _Smoothness;
			float4 _Thresholds;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{			
				const int samples = 16;
				const float step = 3.1415926 * 2.0 / samples;
				float4 col = 0;
				for (int j = 0; j < samples; j++)
				{
					float2 offset = float2(cos(j * step), sin(j * step)) * _Radius / _Size;
					float3 input = tex2D(_MainTex, i.uv + offset);
					if (input.r < _Thresholds.x) 
						col += float4(1,0,0,1);
					else if (input.r < _Thresholds.y)
						col += float4(0,1,0,1);
					else if (input.r < _Thresholds.z)
						col += float4(0,0,1,1);
				}
				col /= samples;
				return col;
			}
		ENDCG
		}

		Pass
		{
			Name "Voronoi"
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			half _Scale, _Power;
			fixed2 _Size;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p,float2(117.12,341.7)),dot(p,float2(269.5,123.3))))*43458.5453);
			}
			float random (float2 st) {
                return frac(sin(dot(st.xy,
                                     float2(12.9898,78.233)))*
                    43758.5453123);
            }
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);
				float2 uv = i.uv;
				uv *= _Size; //Size of the grid
				uv *= _Scale; //Scaling amount (larger number more cells can be seen)
				float2 iuv = floor(uv); //gets integer values no floating point
				float2 fuv = frac(uv); // gets only the fractional part
				float minDist = 1.0;  // minimun distance
				float color = 0;
				for (int y = -2; y <= 2; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						// Position of neighbour on the grid
						float2 neighbour = float2(float(x), float(y));
						// Random position from current + neighbour place in the grid
						float2 pointv = random2(iuv + neighbour);
						// Move the point with time
						pointv = 0.5 + 0.5*sin(0*_Time.z + 6.2236*pointv);//each point moves in a certain way
																		// Vector between the pixel and the point
						float2 diff = neighbour + pointv - fuv;
						// Distance to the point
						float dist = length(diff);
						dist = pow(pow(abs(diff.x), _Power) + pow(abs(diff.y), _Power), 1/_Power);
						// Keep the closer distance
						if (dist < minDist){
							minDist = dist;
							color = random(neighbour +iuv);
						}
					}
				}
				// Draw the min distance (distance field)
				col += color;//minDist * minDist; // squared it to to make edges look sharper
				return col;
			}
		ENDCG
		}
	}
}
