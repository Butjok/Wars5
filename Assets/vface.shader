// https://github.com/przemyslawzaworski/Unity3D-CG-programming/blob/master/surface_shaders/vface.shader
Shader "Vface"
{
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        }
    SubShader
    {
        Cull Off
        CGPROGRAM
        #pragma target 5.0
        #pragma surface SurfaceShader Standard

        sampler2D _MainTex;
     
        struct Input
        {
            float IsFacing:VFACE;
            float2 uv_MainTex;
        };
 
        void SurfaceShader( Input i , inout SurfaceOutputStandard o )
        {
            float4 color = tex2D(_MainTex, i.uv_MainTex);//(i.IsFacing>0) ? float4(1,0,0,1) : float4(0,0,1,1);
            o.Albedo = color.rgb;
            o.Alpha = color.a;
        }
 
        ENDCG
    }
}