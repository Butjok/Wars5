Shader "Custom/wb_unit" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _PlayerColor ("_PlayerColor", Color) = (1,1,1,1)
        _UnownedColor ("_UnownedColor", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MovedColor ("_MovedColor", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "/Assets/Shaders/Utils.cginc"

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color, _PlayerColor, _MovedColor,_UnownedColor;
        float _Moved; 

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed3 c = lerp(_UnownedColor.rgb, _PlayerColor.rgb, _PlayerColor.a) * _Color.rgb ;//tex2D (_MainTex, IN.uv_MainTex) ;
            o.Smoothness = _Glossiness;
            if (_Moved > .5){
                c = Tint(c, 0, .95, .125);
                o.Smoothness = 0;
            }
            o.Albedo = c;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}