Shader "Custom/TrackTrail"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "LightMode" = "Deferred" "Queue" = "Transparent"
        }
        LOD 200
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf  Standard  vertex:vert  alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Noise;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
            float3 worldPos;
        };

        fixed4 _Color;
        float _Length;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            v.vertex = mul(unity_WorldToObject, float4(v.vertex));
            o.uv_MainTex = v.texcoord.xy;
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
            o.Metallic = 0;
            o.Smoothness = 0;
            float noise = tex2D(_Noise, IN.worldPos.xz * 2).r;
            float alpha = saturate(smoothstep(5, 1.5, _Time.y - IN.color.r) * 2 - 1 + noise);
            o.Albedo = _Color;
            o.Alpha = alpha * _Color.a;
        }
        ENDCG
    }
}