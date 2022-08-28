Shader "Custom/LeavesAreaTinted"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint ("_Tint", 2D) = "white" {}        
        _Normal ("_Normal", 2D) = "normal" {}
        _Dist ("_Dist", 2D) = "white" {}
        _SSS ("_SSS", 2D) = "white" {}
        _WithSSS ("_WithSSS", 2D) = "white" {}
        _Indirect ("_Indirect", 2D) = "white" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _GlobalOcclusion ("_GlobalOcclusion", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _SSSIntensity ("_SSSIntensity", Range(0,1)) = 0.0
        
        _Grass ("_Grass", Color) = (1,1,1,1)
        _DarkGrass ("_DarkGrass", Color) = (1,1,1,1)
        _Wheat ("_Wheat", Color) = (1,1,1,1)
        _YellowGrass ("_YellowGrass", Color) = (1,1,1,1)
        
         _Splat ("_Splat", 2D) = "black" {}
    }
    SubShader
    {
            Tags { "RenderType"="Opaque" }
            Cull Off
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard  addshadow 

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex,_Occlusion,_SSS,_Dist,_Normal,_Tint,_GlobalOcclusion,_WithSSS,_Indirect,_Splat;
            half2 _Flip;
            float3 _Grass,_DarkGrass,_Wheat,_YellowGrass;
            float4 _Bounds;

            struct Input {
                float2 uv_MainTex ;
                float2 uv2_GlobalOcclusion ;
                float3 worldPos;
            };

            half _Glossiness;
            half _Metallic,_SSSIntensity;
            fixed4 _Color;

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
                
                
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);

                
                o.Occlusion = 1;//globalOcclusion*localOcclusion *  (1-_SSSIntensity);
                
                o.Emission= tex2D (_Indirect, IN.uv2_GlobalOcclusion)*(1-_SSSIntensity)*c + tex2D (_SSS, IN.uv2_GlobalOcclusion)*_SSSIntensity;
                //o.Albedo = 0;
                //o.Emission = tex2D (_SSS, IN.uv2_SSS);
                
                // Metallic and smoothness come from slider variables
                o.Metallic = 0;
                o.Smoothness =lerp(.1, .25, pow(tex2D (_Occlusion, IN.uv_MainTex),.5));
                //o.Alpha = c.a;
                

                o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));

                //o.Albedo=HueShift(o.Albedo,-.01);
                //o.Emission=HueShift(o.Emission,-.01);

                float2 splatUv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw);
            if (_Flip.x > .5)
                splatUv.x = 1 - splatUv.x;
            if (_Flip.y > .5)
                splatUv.y = 1 - splatUv.y;

            float3 splat = tex2D(_Splat, splatUv);

            o.Albedo = _Grass;
            o.Albedo = lerp(o.Albedo, _DarkGrass, splat.r);
            o.Albedo = lerp(o.Albedo, _Wheat, splat.g);
            o.Albedo = lerp(o.Albedo, _YellowGrass, splat.b);

                o.Albedo*=  tex2D (_Occlusion, IN.uv_MainTex) * (1-_SSSIntensity);

                /*o.Albedo=splat;
                o.Albedo=0;
                o.Albedo.rg=splatUv;*/
            }
            ENDCG
    }
}
