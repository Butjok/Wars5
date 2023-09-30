Shader "Custom/LeavesAreaTinted"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}   
        _Normal ("_Normal", 2D) = "normal" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        
        _Grass ("_Grass", Color) = (1,1,1,1)
        _DarkGrass ("_DarkGrass", Color) = (1,1,1,1)
        _Wheat ("_Wheat", Color) = (1,1,1,1)
        _YellowGrass ("_YellowGrass", Color) = (1,1,1,1)
        
         _Splat2 ("_Splat2", 2D) = "black" {}
         
         _Min ("_Min", Vector) = (0,0,0,1)
         _Size ("_Size", Vector) = (1,1,0,1)
         
         [HDR]_Emissive ("_Emissive", Color) = (1,1,1,1)
         _Offset ("_Offset", Vector) = (0,0,0,0)
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

            sampler2D _MainTex,_Occlusion,_Normal,_GlobalOcclusion, _Splat2, _TileMask;
            half3 _Grass,_DarkGrass,_Wheat,_YellowGrass;
            fixed4x4 _TileMask_WorldToLocal;

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
            };

            half4x4 _WorldToLocal;
            half2 _Min,_Size;
            fixed4 _Emissive;
            fixed4 _Offset;

            struct InstancedRenderingAppdata {
                half4 vertex : POSITION;
                half4 tangent : TANGENT;
                half3 normal : NORMAL;
                half2 texcoord : TEXCOORD0;
                uint inst : SV_InstanceID;
            };
           
            #include "Assets/Shaders/SDF.cginc"
            #include  "Utils.cginc"
            
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
            	StructuredBuffer<half4x4> _Transforms;
            #endif

            void instanced_rendering_vertex2(inout InstancedRenderingAppdata v) { 
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

                const half4x4 transform =  _Transforms[v.inst];

                v.vertex = mul(transform, v.vertex - _Offset);
                v.normal = normalize(mul(transform, v.normal)) ;

            #endif 
            }
          
            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                half4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);
                
                half inputOcclusion = tex2D (_Occlusion, IN.uv_MainTex).r;
                
                o.Occlusion = inputOcclusion;//lerp(inputOcclusion,1,.5);
                
                o.Metallic = 0; 
                o.Smoothness = .2;
                o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));
 
				half2 center = _Min + _Size/2;
				float dist = sdfBox(IN.worldPos.xz-center, _Size/2);
				clip(-(dist-.5));
			   
				half3 localPos = mul(_WorldToLocal, half4(IN.worldPos, 1)).xyz;
				half2 uv = localPos.xz;
				half3 splat = tex2D(_Splat2, uv);
		   
				o.Albedo = _Grass;
				o.Albedo = lerp(o.Albedo, _DarkGrass, splat.r);
				o.Albedo = lerp(o.Albedo, _YellowGrass, splat.b);

                o.Albedo = lerp(o.Albedo, /*tint(o.Albedo, 0, 1.1, .5)*/ o.Albedo/2, 1 - inputOcclusion);
                
                //o.Albedo = _Grass;
                
                float2 uv2 = mul(_TileMask_WorldToLocal, float4(IN.worldPos.xyz, 1)).xz;
                			float tileMask = saturate(tex2D(_TileMask, uv2).r);
                			if (uv2.x < 0 || uv2.x > 1 || uv2.y < 0 || uv2.y > 1)
                				tileMask = 0;
                			o.Emission += _Emissive * tileMask;
                			
                			//o.Emission += _Emissive2*o.Albedo;
                			
                			o.Emission += o.Albedo*.5;
                			o.Albedo*=.5;
            }
            ENDCG
    }
    }