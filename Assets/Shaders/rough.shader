Shader "Custom/rough"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
       [HDR] _Emissive ("Emissive", Color) = (1,1,1,1)
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

        sampler2D _MainTex, _Roughness, _TileMask;
        fixed4x4 _TileMask_WorldToLocal;
        fixed4 _Emissive;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Metallic;
        fixed4 _Color;

        #include "Utils.cginc"
        #include "Assets/Shaders/SDF.cginc"
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = lerp((1-tex2D (_Roughness, IN.uv_MainTex)), .25, .5);
            o.Alpha = c.a;

            /*float3 hsv = RGBtoHSV(o.Albedo);
            hsv.x -= .01;
            //hsv.y *= .95;
            hsv.z *= .75;
            o.Albedo = HSVtoRGB(hsv);*/
            
            half2 dst = abs(IN.worldPos.xz-.5 - round(IN.worldPos.xz-.5));
            half dst2 = min(dst.x, dst.y);
            o.Albedo.rgb = lerp(o.Albedo, o.Albedo*.75, smoothstep(.0125, .0, dst2));
            
            float tileMaskDistance = 1;			
			float2 nearestTile = round(IN.worldPos.xz);
			for (int x = -1; x <= 1; x++)
			for (int y = -1; y <= 1; y++) {
				float2 pos = nearestTile + float2(x, y);
				float selected = tex2D(_TileMask, mul(_TileMask_WorldToLocal, float4(pos.x, 0, pos.y, 1)).xz).r;
				if (selected > .5)
					 tileMaskDistance = min(tileMaskDistance, sdfBox(IN.worldPos.xz - pos, 0.5));
			}

			//o.Albedo = saturate(tileMaskDistance);
			
			float3 tileMaskEmission = 0;
			tileMaskEmission += _Emissive * smoothstep(0.05, -.025, tileMaskDistance);
			tileMaskEmission += 3.3*_Emissive * smoothstep(0.025, 0.0125, abs(tileMaskDistance - .025));
			
			o.Emission = tileMaskEmission ;
			o.Albedo  = lerp(o.Albedo, o.Emission, (o.Emission.r + o.Emission.g + o.Emission.b) / 1);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
