Shader "Custom/rough"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
       [HDR] _Emissive ("Emissive", Color) = (1,1,1,1)
    	_HSVTweak ("HSV Tweak", Vector) = (0, 1, 1, 1)
    	_SeaLevel ("Sea Level", Float) = 0.5
    	_SeaSharpness ("Sea Sharpness", Float) = 0.5
    	_SeaColor ("Sea Color", Color) = (0,0,1,1)
    	_SeaHSVTweak ("Sea HSV Tweak", Vector) = (0, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _Roughness, _TileMask;
        fixed4x4 _TileMask_WorldToLocal;
        fixed4 _Emissive;
        float4 _HSVTweak;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Metallic;
        fixed4 _Color;

        float _SeaLevel;
        float _SeaSharpness;
        fixed4 _SeaColor;
        float4 _SeaHSVTweak;

        #include "Utils.cginc"
        #include "Assets/Shaders/SDF.cginc"
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #include "Assets/Shaders/ClassicNoise.cginc"

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
        	
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = 0;
            o.Alpha = c.a;

            
            half2 dst = abs(IN.worldPos.xz-.5 - round(IN.worldPos.xz-.5));
            half dst2 = min(dst.x, dst.y);



        	float3 hsv = RGBtoHSV(o.Albedo);
        	hsv.x += _HSVTweak.x;
        	hsv.y *= _HSVTweak.y;
        	hsv.z *= _HSVTweak.z;
        	o.Albedo = HSVtoRGB(hsv);

        	

        	float sea = smoothstep(_SeaLevel - _SeaSharpness, _SeaLevel + _SeaSharpness, IN.worldPos.y);
            {
	            float3 hsv = RGBtoHSV(o.Albedo);
            	hsv.x += _SeaHSVTweak.x;
            	hsv.y *= _SeaHSVTweak.y;
            	hsv.z *= _SeaHSVTweak.z;
            	o.Albedo = lerp(o.Albedo, HSVtoRGB(hsv), sea);
            }

        	o.Albedo *= _Color;
        	
        }
        ENDCG
    }
    FallBack "Diffuse"
}
