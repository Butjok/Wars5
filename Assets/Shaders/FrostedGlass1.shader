Shader "Unlit/FrostedGlass1" {
	Properties {
		_Size ("Blur", Vector) = (4,4,0,0)
			[HideInInspector] _MainTex ("Masking Texture", 2D) = "white" {}
		_AdditiveColor ("Additive Tint color", Color) = (0, 0, 0, 0)
			_MultiplyColor ("Multiply Tint color", Color) = (1, 1, 1, 1)
	}
	
	CGINCLUDE
	
	static const int kernelSize = 71;
	static const float weights[] = {
		0.0003807622546203967,
        0.0004906075713239304,
        0.0006275146475665883,
        0.0007967512511978496,
        0.00100422471517359,
        0.0012564590020428404,
        0.0015605403549293346,
        0.0019240260617353507,
        0.002354811576005856,
        0.002860952443599696,
        0.003450439210754145,
        0.004130925733511178,
        0.004909414025601617,
        0.005791901878532406,
        0.0067830028202928005,
        0.007885551356100562,
        0.00910020962318819,
        0.010425094330359048,
        0.011855444869280767,
        0.013383354515985567,
        0.014997586460270709,
        0.016683494839778542,
        0.018423067929526617,
        0.020195106163377736,
        0.021975541872002756,
        0.02373790075975841,
        0.025453897567563874,
        0.02709415052865096,
        0.028628991630285653,
        0.030029342884658913,
        0.031267623307302275,
        0.03231864756219318,
        0.033160475617137634,
        0.033775173480835176,
        0.0341494482206322,
        0.03427512586844697,
        0.0341494482206322,
        0.033775173480835176,
        0.03316047561713764,
        0.03231864756219318,
        0.03126762330730228,
        0.030029342884658913,
        0.028628991630285663,
        0.02709415052865097,
        0.025453897567563874,
        0.023737900759758415,
        0.021975541872002756,
        0.020195106163377743,
        0.018423067929526617,
        0.016683494839778552,
        0.014997586460270709,
        0.013383354515985574,
        0.011855444869280767,
        0.010425094330359048,
        0.0091002096231882,
        0.007885551356100562,
        0.00678300282029281,
        0.005791901878532406,
        0.004909414025601621,
        0.004130925733511178,
        0.0034504392107541477,
        0.002860952443599696,
        0.0023548115760058585,
        0.0019240260617353507,
        0.0015605403549293346,
        0.0012564590020428422,
        0.00100422471517359,
        0.0007967512511978507,
        0.0006275146475665883,
        0.0004906075713239308,
        0.0003807622546203967,
	};
	
	ENDCG

	Category {

		// We must be transparent, so other objects are drawn before this one.
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" }


		SubShader
		{
			// Vertical blur
			GrabPass
			{
				"_VBlur"
			}

			Pass
			{
				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord: TEXCOORD0;
					};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uvmain : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert (appdata_t v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
#else
					float scale = 1.0;
#endif

					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;

					o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);

					return o;
				}

				sampler2D _VBlur;
				float4 _VBlur_TexelSize;
				float2 _Size;
				float4 _AdditiveColor;
				float4 _MultiplyColor;

				half4 frag( v2f i ) : COLOR
				{
					half4 sum = half4(0,0,0,0);
					half input = tex2D(_MainTex, i.uvmain).a;
					half radius = input * _Size.y;

#define GRABPIXEL(weight,kernely) tex2D( _VBlur, float2(i.uvgrab.x, i.uvgrab.y + _VBlur_TexelSize.y * kernely * radius)) * weight

					for (int j = 0; j < kernelSize; j++) {
						int offset = j - kernelSize / 2;
						sum += GRABPIXEL(weights[j], offset);
					}

					half4 color = half4(sum.rgb, 1);
					half4 tinted = sum * _MultiplyColor + _AdditiveColor;
					return lerp(color, tinted, input);
				}
				ENDCG
			}
		
			// Horizontal blur
			GrabPass
			{
				"_HBlur"
			}
			/*
			   ZTest Off
			   Blend SrcAlpha OneMinusSrcAlpha
			 */

			Cull Off
				Lighting Off
				ZWrite Off
				ZTest [unity_GUIZTestMode]
				Blend SrcAlpha OneMinusSrcAlpha

				Pass
				{
					CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

						struct appdata_t {
							float4 vertex : POSITION;
							float2 texcoord : TEXCOORD0;
						};

					struct v2f {
						float4 vertex : POSITION;
						float4 uvgrab : TEXCOORD0;
						float2 uvmain : TEXCOORD1;
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;

					v2f vert (appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);

#if UNITY_UV_STARTS_AT_TOP
						float scale = -1.0;
#else
						float scale = 1.0;
#endif

						o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
						o.uvgrab.zw = o.vertex.zw;

						o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
						return o;
					}

					sampler2D _HBlur;
					float4 _HBlur_TexelSize;
					float2 _Size;
					float4 _AdditiveColor;
					float4 _MultiplyColor;

					half4 frag( v2f i ) : COLOR
					{
						half4 sum = half4(0,0,0,0);
						half input = tex2D(_MainTex, i.uvmain).a;
						half radius = input * _Size.x; 

#define GRABPIXEL(weight,kernelx) tex2D( _HBlur, float2(i.uvgrab.x + _HBlur_TexelSize.x * kernelx * radius, i.uvgrab.y)) * weight

						for (int j = 0; j < kernelSize; j++) {
							int offset = j - kernelSize / 2;
							sum += GRABPIXEL(weights[j], offset);
						}

						half4 color = half4(sum.rgb, 1);
						half4 tinted = sum * _MultiplyColor + _AdditiveColor;
						return lerp(color, tinted, input);
					}
					ENDCG
				}

			
		}
	}
}
