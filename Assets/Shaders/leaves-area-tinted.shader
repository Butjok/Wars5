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
            #pragma surface surf Standard vertex:instanced_rendering_vertex2  addshadow 

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 5.0

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

            struct InstancedRenderingAppdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;

                uint inst : SV_InstanceID;
            };
            #include "Assets/Shaders/InstancedRendering.cginc"

            void instanced_rendering_vertex2(inout InstancedRenderingAppdata v) {
            #ifdef SHADER_API_D3D11

                const float4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

                v.vertex = mul(transform, v.vertex);
                //v.tangent = mul(transform, v.tangent);
                v.normal = normalize(mul(transform, v.normal)) ;

                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                
                float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
                if (dot(v.normal,lightDirection)<0)
                    v.normal = -v.normal;    

            #endif 
            }
            
            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                
                
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);

                
                o.Occlusion = 1;//globalOcclusion*localOcclusion *  (1-_SSSIntensity);
                
                //o.Emission= tex2D (_Indirect, IN.uv2_GlobalOcclusion)*(1-_SSSIntensity)*c + tex2D (_SSS, IN.uv2_GlobalOcclusion)*_SSSIntensity;
                //o.Albedo = 0;
                //o.Emission = tex2D (_SSS, IN.uv2_SSS);
                
                // Metallic and smoothness come from slider variables
                o.Metallic = 0;
                //o.Smoothness =lerp(.1, .25, pow(tex2D (_Occlusion, IN.uv_MainTex),.5));
                half globalOcclusion =tex2D (_GlobalOcclusion, IN.uv2_GlobalOcclusion).r; 
                o.Smoothness =lerp(.15, .3, pow(tex2D (_Occlusion, IN.uv_MainTex),1)) ;//* (globalOcclusion);
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

                o.Albedo*=  tex2D (_Occlusion, IN.uv_MainTex);// * (1-_SSSIntensity);
                //o.Emission = o.Albedo*.125;

                /*o.Albedo=splat;
                o.Albedo=0;
                o.Albedo.rg=splatUv;*/

                o.Alpha=.5;
            }
            ENDCG
    }
}
