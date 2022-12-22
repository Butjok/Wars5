Shader "Custom/TerrainTilesTest" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _WaveSpeed ("_WaveSpeed", Range(0,100)) = 0.0
        _WaveBorderSize ("_WaveBorderSize", Range(0,100)) = 10
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half4 _Bounds;
        half _WaveStartTime;
        half _WaveSpeed;
        half _WaveBorderSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            int2 position = round(IN.worldPos.xz);
            int2 min = round(_Bounds.xy);
            int2 max = round(_Bounds.zw);
            
            o.Albedo = half3(0,0,0);
            if (position.x >= min.x && position.y >= min.y &&
                position.x <= max.x && position.y <= max.y)
            {
                int2 relativePosition = position - min;
                int2 size = max - min + int2(1,1);
                float2 step = float2(1,1) / size;
                float2 uv = frac(relativePosition * step + step / 2);
                half distance = round(tex2D(_MainTex, uv).r);
                if (distance >= 0 || distance == -2)
                {
                    half time = (_Time.y - _WaveStartTime) * _WaveSpeed;
                    o.Albedo.r = smoothstep(0, -_WaveBorderSize, distance - time);   
                }
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}