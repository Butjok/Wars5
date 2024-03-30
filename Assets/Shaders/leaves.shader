Shader "Custom/leaves"
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
        
        _SplatMap ("SplatMap", 2D) = "white" {}
    }
    SubShader
    {
            Tags { "RenderType"="Opaque" }
            Cull Off
            LOD 200

            CGPROGRAM
            
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard vertex:instanced_rendering_vertex addshadow 

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            #include "Assets/Shaders/Utils.cginc"
            
            struct InstancedRenderingAppdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;

                uint inst : SV_InstanceID;
            };
            
            struct InstancedRenderingTransform {
                half4x4 mat;
            };

            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
                StructuredBuffer<InstancedRenderingTransform> _Transforms;
            #endif

            void instanced_rendering_vertex(inout InstancedRenderingAppdata v) {
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

                const half4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

                half intensity = length(v.vertex.xz);
                
                v.vertex = mul(transform, v.vertex);
                //v.tangent = mul(transform, v.tangent);
                v.normal = normalize(mul(transform, v.normal));

                half wind = sin(_Time.y*7 - v.vertex.x*4)/2+.5;
                v.vertex += wind * intensity* float4(1,0,0,0) * sin(_Time.y*4 + v.vertex.z*15)*.03;
                v.vertex += wind * intensity* float4(0,0,1,0) * sin(_Time.y*5 + v.vertex.x*12.4535)*.03;

            #endif
            }

            
            sampler2D _MainTex,_Occlusion,_SSS,_Dist,_Normal,_Tint,_GlobalOcclusion,_WithSSS,_Indirect;

            struct Input {
                float2 uv_MainTex ;
                float2 uv2_GlobalOcclusion ;
                float3 worldPos;
            };

            half _Glossiness;
            half _Metallic,_SSSIntensity;
            fixed4 _Color;
            sampler2D _TileMask;
            sampler2D _SplatMap;
            fixed4x4 _TileMask_WorldToLocal;

            #include "Assets/Shaders/SDF.cginc"

            float3 shift(float3 color)
            {
                float3 hsv = RGBtoHSV(color);
                //hsv.x += .02;
                hsv.y *= .99;
                return HSVtoRGB(hsv);
            }
    
            float3 tint(float3 color, float hueShift, float saturationShift, float valueShift){
                float3 hsv = RGBtoHSV(color);
                hsv.x += hueShift;
                hsv.y *= saturationShift;
                hsv.z *= valueShift;
                return HSVtoRGB(hsv);
            }
            
            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                //float2 uv2 = mul(_TileMask_WorldToLocal, float4(IN.worldPos.xyz, 1)).xz;
				//float tileMask = saturate(tex2D(_TileMask, uv2).r);
				//if (uv2.x < 0 || uv2.x > 1 || uv2.y < 0 || uv2.y > 1)
				//	tileMask = 0;
				//clip(-(tileMask - 0.5));
                
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);

                half globalOcclusion =tex2D (_GlobalOcclusion, IN.uv2_GlobalOcclusion).r; 
                half localOcclusion= tex2D (_Occlusion, IN.uv_MainTex).r;
                
                half2 position = IN.worldPos.xz; 
                // Albedo comes from a texture tinted by color
                c.rgb = tex2D (_Tint,  position/5)  * localOcclusion;

                half dist = tex2D (_Dist, IN.uv_MainTex).r;
                half distI = smoothstep(.1, 0, dist);

                half aoMask = 1-tex2D (_Occlusion, IN.uv_MainTex);
                
                o.Albedo = tex2D (_Tint, IN.uv_MainTex) * (1-_SSSIntensity);
                o.Albedo = lerp(o.Albedo, tint(o.Albedo, 0, 1.1, .5), aoMask);

                o.Occlusion = 1;//globalOcclusion*localOcclusion *  (1-_SSSIntensity);
                
                o.Emission= tex2D (_Indirect, IN.uv2_GlobalOcclusion)*(1-_SSSIntensity)*c + tex2D (_SSS, IN.uv2_GlobalOcclusion)*_SSSIntensity;
                //o.Albedo = 0;
                //o.Emission = tex2D (_SSS, IN.uv2_SSS);
                
                // Metallic and smoothness come from slider variables
                o.Metallic = 0;
                o.Smoothness = lerp(0, .125, pow(tex2D (_Occlusion, IN.uv_MainTex),1)) * (globalOcclusion);
                //o.Alpha = c.a;
                

                o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));

                /*o.Albedo=hue_shift(o.Albedo,-.04);
                o.Emission=hue_shift(o.Emission,-.04);*/

                o.Emission = lerp(o.Emission, tint(o.Emission, 0, 1.1, .5), aoMask);
                
                o.Albedo = shift(o.Albedo);
                o.Emission = shift(o.Emission);

                float3 albedoHSV = RGBtoHSV(o.Albedo);
                albedoHSV.x -= .125; //h
                albedoHSV.y *= 2.5; //s
                //albedoHSV.z *= 0.75; //v

                float3 emissionHSV = RGBtoHSV(o.Emission);
                emissionHSV.z *= .75;
                o.Emission = HSVtoRGB(emissionHSV);
                o.Emission *= .5;

                o.Emission *= _Color;
                o.Emission=0;

                float4 splat =  tex2D(_SplatMap, IN.worldPos.xz/30);
                //albedoHSV.r -= (splat.r)*.0125;
                
                o.Albedo = HSVtoRGB(albedoHSV);
                o.Albedo = _Color;
                //o.Albedo = splat.rgb;

                o.Smoothness=0;
            }
            ENDCG
    }
}
