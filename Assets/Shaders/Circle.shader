Shader "Unlit/Circle"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
        _SpeedX ("_Speed X", Range(-1, 1)) = 0.1
        _SpeedY ("_Speed Y", Range(-1, 1)) = 0.1
        _Color ("Tint", Color) = (1,1,1,1)
 
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
 
 
 _Size ("_Size", Float) = 100
 _Thickness ("_Thickness", Float) = 2.5
 _Smoothness ("_Smoothness", Float) = 1
 _Offset ("_Offset", Float) = 1
 
        _ColorMask ("Color Mask", Float) = 15
 
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
 
        SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
 
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
 
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
 
        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
 
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
 
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
 
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float2 texcoord1  : TEXCOORD2;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            fixed _SpeedX;
            fixed _SpeedY;
            fixed _Thickness, _Size, _Smoothness, _Offset;
 
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
 
                // pan texture by offseting uv's over time
                OUT.texcoord.xy = v.texcoord.xy;// + frac(_Time.y * float2(_SpeedX, _SpeedY));
 		OUT.texcoord1.xy = v.texcoord1.xy;
 
                OUT.color = v.color * _Color;
                return OUT;
            }
 
            sampler2D _MainTex;
            float _StartTime;
            
            float2 _V0,_V1,_V2,_V3;
            
            float sdSegment( float2 p, float2 a, float2 b )
            {
                float2 pa = p-a, ba = b-a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                return length( pa - ba*h );
            }
 
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) ) * IN.color;
                
                float2 position = IN.texcoord * _Size;
                float distanceToCenter = length(position-float2(_Size/2, _Size/2)) + _Offset;
                float radius = _Size / 2;
                float distanceToCircle = abs(distanceToCenter - radius);
                float alpha =1- smoothstep(_Thickness - _Smoothness, _Thickness + _Smoothness, distanceToCircle );
                
                color.a = alpha;
 
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
 
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
 
                return color;
            }
        ENDCG
        }
    }
    FallBack "UI/Default"
}