Shader "Hidden/Custom/VignetteBlur"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		// Constants
#define center float2(0.5, 0.5)
#define outerRadius 1
#define innerRadius 0.33

#define darkness 0.85

#define redAberration 10.0
#define greenAberration 0.0
#define blueAberration -10.0

	#define power 4.0

	sampler2D _MainTex;

// Unity3D post-processing shader function
half4 Frag (VaryingsDefault input) : SV_Target
{
	float2 difference = abs(input.texcoord - center);
	float distance = pow(pow(difference.x, power) + pow(difference.y, power), 1/power);
	float intensity = smoothstep(innerRadius, outerRadius, distance);
	float blurSize = intensity * 0.0025;
	float4 color = 0;
	for (int i = -1; i <= 1; i++)
	{
		for (int j = -1; j <= 1; j++)
		{
			color += tex2D(_MainTex, input.texcoord + float2(i, j) * blurSize);
		}
	}
	return color / 9;
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
