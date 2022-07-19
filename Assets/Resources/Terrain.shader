Shader "Custom/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Grass ("_Grass", 2D) = "white" {}
        _Grid ("_Grid", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Size ("_Size", Range(0,64)) = 0.0
        _ShowTime ("_ShowTime", Range(0,64)) = 0.0
        _Rounding ("_Rounding", Range(-1,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _Grass,_Grid;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic,_ShowTime,_Rounding;
        fixed4 _Color;

        #define SIZE 64
        int _Size=3;
         int2 _Positions[SIZE] = {
            int2(0,0),
            int2(1,1),
            int2(2,2),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999)
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        half sdfBox(half2 p, half2 size)
        {
            half2 d = abs(p) - size;  
	        return length(max(d, half2(0,0))) + min(max(d.x, d.y), 0.0);
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 position = IN.worldPos.xz;
            int2 cell = round(position);

            half minDist = 999;
            for (int i = 0; i < _Size; i++)
            {
                half dist= sdfBox(position - _Positions[i], half2(.5,.5));
                if(minDist>dist)
                    minDist = dist;
            }
            half highlightIntensity = smoothstep(.01,0.005,minDist-_Rounding);
            half border = smoothstep(.025,0.02,abs(0-(minDist-_Rounding)));
            half3 highlight = half3(1,1,1)/3;
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_Grass, position);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = c.a;

            o.Emission=border+highlightIntensity*tex2D (_Grid, position-.5)*10*c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
