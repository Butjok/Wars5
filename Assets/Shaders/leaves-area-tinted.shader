Shader "Custom/LeavesAreaTinted"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}   
        _Normal ("_Normal", 2D) = "normal" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _GlobalOcclusion ("_GlobalOcclusion", 2D) = "white" {}
        
        _Grass ("_Grass", Color) = (1,1,1,1)
        _DarkGrass ("_DarkGrass", Color) = (1,1,1,1)
        _Wheat ("_Wheat", Color) = (1,1,1,1)
        _YellowGrass ("_YellowGrass", Color) = (1,1,1,1)
        
         _Splat ("_Splat", 2D) = "black" {}
         _Splat2 ("_Splat2", 2D) = "black" {}
         
         _Min ("_Min", Vector) = (0,0,0,1)
         _Size ("_Size", Vector) = (1,1,0,1)
    }
    SubShader
    {
            Tags { "RenderType"="Opaque" }
            //Cull Off
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard vertex:instanced_rendering_vertex2  addshadow 

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 5.0

            sampler2D _MainTex,_Occlusion,_Normal,_GlobalOcclusion, _Splat2;
            half3 _Grass,_DarkGrass,_Wheat,_YellowGrass;

            struct Input {
                float2 uv_MainTex ;
                float2 uv2_GlobalOcclusion ;
                float3 worldPos;
                float IsFacing:VFACE;
            };

            half4x4 _WorldToLocal;
            half2 _Min,_Size;

            struct InstancedRenderingAppdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                uint inst : SV_InstanceID;
            };
            
            #include "Assets/Shaders/InstancedRendering.cginc"
            #include "Assets/Shaders/SDF.cginc"
            #include  "Utils.cginc"

            void instanced_rendering_vertex2(inout InstancedRenderingAppdata v) { 
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

                const half4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

                v.vertex = mul(transform, v.vertex);
                v.normal = normalize(mul(transform, v.normal)) ;

            #endif 
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
                half4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);
                
                o.Occlusion = lerp(tex2D (_Occlusion, IN.uv_MainTex),1,.5);
                
                o.Metallic = 0; 
                o.Smoothness = .2;
                o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));
 
				/*half2 center = _Min + _Size/2;
				float dist = sdfBox(IN.worldPos.xz-center, _Size/2);
				clip(-(dist-.5));*/
			   
				half3 localPos = mul(_WorldToLocal, half4(IN.worldPos, 1)).xyz;
				half2 uv = localPos.xz;
				half3 splat = tex2D(_Splat2, uv);
		   
				o.Albedo = _Grass;
				o.Albedo = lerp(o.Albedo, _DarkGrass, splat.r);
				o.Albedo = lerp(o.Albedo, _YellowGrass, splat.b);

                o.Albedo = lerp(o.Albedo, tint(o.Albedo, 0, 1.1, .5), 1 - tex2D (_Occlusion, IN.uv_MainTex).r);
            }
            ENDCG
    }
}
