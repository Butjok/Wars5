Shader "Custom/Minimap"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _Plain ("_Plain", 2D) = "white" {}
        _Road ("_Road", 2D) = "white" {}
        _Sea ("_Sea", 2D) = "white" {}
        _Mountain ("_Mountain", 2D) = "white" {}
        
        _Plant ("_Plant", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex,_Plain,_Road,_Sea,_Mountain,_Plant;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #define EPSILON .1

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            half id = IN.color.a;

            half plain = abs(id - 0) < EPSILON;
            half road = abs(id - 1) < EPSILON;
            half sea = abs(id - 2) < EPSILON;
            half mountain = abs(id - 3) < EPSILON;

            half building = id > 3.5;
            half city = abs(id - 4) < EPSILON;
            half hq = abs(id - 5) < EPSILON;
            half plant = abs(id - 6) < EPSILON;
            half airport = abs(id - 7) < EPSILON;

            o.Albedo = 0;
            o.Albedo += plain * tex2D(_Plain, IN.uv_MainTex);
            o.Albedo += road * tex2D(_Road, IN.uv_MainTex);;
            o.Albedo += sea * tex2D(_Sea, IN.uv_MainTex);;
            o.Albedo += mountain * tex2D(_Mountain, IN.uv_MainTex);;

            float4 plantRGBA = tex2D(_Plant, IN.uv_MainTex);
            o.Albedo += plant * lerp(plantRGBA.rgb, IN.color.rgb, plantRGBA.a);

            o.Emission += tex2D(_MainTex, IN.uv_MainTex).a * o.Albedo*0.5;
            
            //o.Albedo = id/7;
            //o.Albedo=0;
            //o.Albedo.rg=frac(IN.uv_MainTex);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}