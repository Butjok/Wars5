Shader "Custom/Unit"
{
    Properties
    {
        _PlayerColor ("_PlayerColor", Color) = (1,1,1,1)
        _MainTex ("_MainTex", 2D) = "white" {}
        _BounceLight ("_BounceLight", 2D) = "black" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "black" {}
        _Normal ("_Normal", 2D) = "bump" {}
        _Metallic ("_Metallic", 2D) = "black" {}
        _HueShift ("_HueShift", Range(0,1)) = 1.0
        _BounceIntensity ("_BounceIntensity", Range(0,10)) = 1.0
        _Offset ("_Offset", Vector) = (0, 0, .5, 1)
        _OffsetIntensity ("_OffsetIntensity", Range(0,10)) = 0
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

        sampler2D _MainTex,_Occlusion,_Normal,_Roughness,_Metallic,_BounceLight;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _PlayerColor,_Offset;
        half _Selected,_HueShift,_BounceIntensity,_OffsetIntensity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        half3 HueShift ( half3 Color, in float Shift)
        {
            half3 P = half3(0.55735,0.55735,0.55735)*dot(half3(0.55735,0.55735,0.55735),Color);
            
            half3 U = Color-P;
            
            half3 V = cross(half3(0.55735,0.55735,0.55735),U);    

            Color = U*cos(Shift*6.2832) + V*sin(Shift*6.2832) + P;
            
            return Color;
        }

        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            half2 uv = IN.uv_MainTex + _Offset.xy*_OffsetIntensity;

            half4 bounce = tex2D (_BounceLight, uv);
            
            // Albedo comes from a texture tinted by color
            fixed3 c = HueShift(tex2D (_MainTex, uv),_HueShift) * _PlayerColor;
            o.Albedo = c.rgb;
            //o.Albedo=tex2D (_Normal, IN.uv_MainTex);
            // Metallic and smoothness come from slider variables
            o.Metallic = tex2D (_Metallic, uv);
            o.Smoothness = 1- tex2D (_Roughness, uv);
            //o.Alpha = c.a;
            o.Occlusion= tex2D (_Occlusion, uv);
            o.Normal=UnpackNormal(tex2D (_Normal, uv));

            
            o.Emission=c.rgb*HueShift(bounce.rgb,_HueShift)*_BounceIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
