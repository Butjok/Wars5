Shader "Hidden/Custom/CameraFading"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _SquareSize;
	float _Smoothness;
	float _Progress;
	float _ProgressSmoothness;
	float _YContribution;
	float _Invert;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 position = i.texcoord*_ScreenParams.xy;
		float2 squarePosition = position % _SquareSize;
		float distanceToSquareCenter = length(squarePosition - float2(_SquareSize,_SquareSize)/2)+.5;
		_Progress += i.texcoord.y * _YContribution;
		float radius = smoothstep(
				_Progress-_ProgressSmoothness, 
				_Progress+_ProgressSmoothness,
				i.texcoord.x*(1-2*_ProgressSmoothness)+_ProgressSmoothness) * _SquareSize*sqrt(2);
		if(_Invert > .5)
			radius = _SquareSize*sqrt(2) - radius;
		float insideCircle = smoothstep(radius - _Smoothness, radius + _Smoothness, distanceToSquareCenter);
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		return lerp(lerp(color, float4(0,0,0,1), radius/_SquareSize*2), color, insideCircle);
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
