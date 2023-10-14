Shader "Hidden/Flip"
{
HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;
		uv.x = 1-uv.x;
		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	}

	ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

				Pass
				{
					HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment Frag

						ENDHLSL
				}
		}
}