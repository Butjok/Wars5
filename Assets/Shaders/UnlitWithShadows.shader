Shader "UnlitWithShadows"
{
    Properties
    {
        _TextureMap ("Texture", 2D) = "" {}
        _Indirect ("_Indirect", 2D) = "" {}
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex VSMain
            #pragma fragment PSMain
            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"
           
            sampler2D _TextureMap, _Indirect;
 
            struct SHADERDATA
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 _ShadowCoord : TEXCOORD1;
            };
 
            float4 ComputeScreenPos (float4 p)
            {
                float4 o = p * 0.5;
                return float4(float2(o.x, o.y*_ProjectionParams.x) + o.w, p.zw);
            }
 
            SHADERDATA VSMain (float4 vertex:POSITION, float2 uv:TEXCOORD0)
            {
                SHADERDATA vs;
                vs.position = UnityObjectToClipPos(vertex);
                vs.uv = uv;
                vs._ShadowCoord = ComputeScreenPos(vs.position);
                return vs;
            }
 
            float4 PSMain (SHADERDATA ps) : SV_TARGET
            {
            	float4 diffuseColor = tex2D(_TextureMap, ps.uv);
            	float4 indirect = tex2D(_Indirect, ps.uv);
            	//return diffuseColor;
            	return lerp(diffuseColor,indirect,step(0.5,SHADOW_ATTENUATION(ps)));
            	//return diffuseColor * step(0.5, ) + indirect;
                //return lerp(float4(0,0,0,1), color, step(0.5, SHADOW_ATTENUATION(ps)));
            }
           
            ENDCG
        }
       
        Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }
            CGPROGRAM
            #pragma vertex VSMain
            #pragma fragment PSMain
 
            float4 VSMain (float4 vertex:POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }
 
            float4 PSMain (float4 vertex:SV_POSITION) : SV_TARGET
            {
                return 0;
            }
           
            ENDCG
        }
    }
}