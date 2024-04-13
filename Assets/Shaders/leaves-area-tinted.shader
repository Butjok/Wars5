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
    	_Forest ("_Forest", Color) = (1,1,1,1)
        
         _Splat2 ("_Splat2", 2D) = "black" {}
         
         _Min ("_Min", Vector) = (0,0,0,1)
         _Size ("_Size", Vector) = (1,1,0,1)
         
         [HDR]_Emissive ("_Emissive", Color) = (1,1,1,1)
         _Offset ("_Offset", Vector) = (0,0,0,0)
    	
    	_ForestMask ("_ForestMask", 2D) = "black" {}
    	
    	_Lod ("_Lod", Float) = 0
    	
    	_SpotMask ("_SpotMask", 2D) = "white" {}
    	_SpotColor ("_SpotColor", Color) = (1,1,1,1)
    	
    	_DeepSeaLevel ("_DeepSeaLevel", Float) = 0
    	_DeepSeaSharpness ("_DeepSeaSharpness", Float) = 0
    	_DeepSeaColor ("_DeepSeaColor", Color) = (1,1,1,1)
    	
    	_Erosion ("_Erosion", 2D) = "black" {}
    	_GrassTint ("_TintMask", 2D) = "black" {}
    	
    	_Smoothness ("_Smoothness", Range(0,1)) = 0.5
    	
    	_FlowersAlpha ("_FlowersAlpha", 2D) = "black" {}
    	_FlowerColor ("_FlowerColor", Color) = (1,1,1,1)
    	
    	_NormalWrap ("_NormalWrap", Float) = 0.5
    	[Toggle(WIND)] _Wind ("Wind", Float) = 0
    }
    SubShader
    {
            Tags { "RenderType"="Opaque" }
            //Cull Off
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard vertex:instanced_rendering_vertex2  addshadow
            #pragma shader_feature   WIND

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 5.0


            #include "Assets/Shaders/ClassicNoise.cginc"
            
            sampler2D _MainTex,_Occlusion,_Normal,_GlobalOcclusion, _Splat2, _TileMask, _ForestMask, _SpotMask, _GrassTint;
            half3 _Grass,_DarkGrass,_Wheat,_YellowGrass,_Forest;
            fixed4x4 _TileMask_WorldToLocal, _ForestMask_WorldToLocal;
            half _Lod;
            fixed4 _SpotColor, _SpotMask_ST, _GrassTint_ST;

            float _DeepSeaLevel, _DeepSeaSharpness;
            half3 _DeepSeaColor;

            sampler2D _FlowersAlpha;
            fixed4 _FlowerColor;

            half _Smoothness;
            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
            };

            half4x4 _Splat_WorldToLocal;
            half2 _Min,_Size;
            fixed4 _Emissive, _Offset;

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

            half _NormalWrap;

            void instanced_rendering_vertex2(inout InstancedRenderingAppdata v) { 
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

                const half4x4 transform =  _Transforms[v.inst];


            	#if WIND
half intensity = length(v.vertex.xz);
            half wind = sin(_Time.y*7 - v.vertex.x*4)/2+.5;
                v.vertex += wind * intensity* float4(1,0,0,0) * sin(_Time.y*4 + v.vertex.z*15)*.04;
                v.vertex += wind * intensity* float4(0,0,1,0) * sin(_Time.y*5 + v.vertex.x*12.4535)*.04;
            	#endif
            	
            	
                v.vertex = mul(transform, v.vertex + _Offset);
            	v.normal = lerp(v.normal, float3(0,-1,0), _NormalWrap);
                v.normal = normalize(mul(transform, v.normal)) ;
                //v.normal = lerp(v.normal, float3(0,1,0), _NormalWrap);

            

            #endif 
            }

            float3 tint(float3 color, float hueShift, float saturationShift, float valueShift){
                float3 hsv = RGBtoHSV(color);
                hsv.x += hueShift;
                hsv.y *= saturationShift;
                hsv.z *= valueShift;
                return HSVtoRGB(hsv);
            }

			sampler2D _Erosion;
            float4x4 _Erosion_WorldToLocal;

float3 Overlay(float3 bg, float3 fg) {
	        return bg < 0.5 ? (2.0 * bg * fg) : (1.0 - 2.0 * (1.0 - bg) * (1.0 - fg));
        }
            
            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                half4 c = tex2D (_MainTex, IN.uv_MainTex);
                clip(c.a-.5);
                
                //half inputOcclusion = tex2D (_Occlusion, IN.uv_MainTex).r;
                
                //o.Occlusion = lerp(inputOcclusion,1,.5);

float erosion =  tex2D(_Erosion, mul(_Erosion_WorldToLocal, float4(IN.worldPos.xyz, 1)).xz).r;
            	
                o.Metallic = 0; 
                o.Smoothness = _Smoothness;
            	o.Occlusion=1;
                o.Normal = UnpackNormal(tex2D (_Normal, IN.uv_MainTex));
 
				//half2 center = _Min + _Size/2;
				//float dist = sdfBox(IN.worldPos.xz-center, _Size/2);
				//clip(-(dist-.5));
			   
				half3 localPos = mul(_Splat_WorldToLocal, half4(IN.worldPos, 1)).xyz;
				half2 uv = localPos.xz;
				half4 splat = tex2D(_Splat2, uv);
		   
				o.Albedo = _Grass;
				o.Albedo = lerp(o.Albedo, _DarkGrass, splat.r);
				o.Albedo = lerp(o.Albedo, _YellowGrass, splat.b);

	fixed4 grassTint = tex2D (_GrassTint, TRANSFORM_TEX(IN.worldPos.xz, _GrassTint) );
	float3 hsv = RGBtoHSV(o.Albedo);
        				hsv.y *= 1.33;
        				hsv.z *= .85; // value
        				o.Albedo = lerp(o.Albedo, HSVtoRGB(hsv), grassTint);

            	{
        			float3 grassTint = 0;
        			if (erosion < .4)
        				grassTint = .4;
        			else
        				grassTint = lerp(.4, float3(.75,.75,.5), (erosion - .4) / (1 - .4));
        			//o.Albedo = grassTint;
        			o.Albedo = Overlay(o.Albedo, grassTint);
        		}

            	/*float3 hsv = RGBtoHSV(o.Albedo);
            	hsv.z *= 1.5;
            	o.Albedo = HSVtoRGB(hsv);*/
            	
				//o.Albedo = lerp(o.Albedo, _Wheat, splat.a);

float flowerAlpha = tex2D(_FlowersAlpha, IN.worldPos.xz * 0.25).r;

//half flowerAo = tex2D(_FlowersAo, IN.uv_FlowersAlpha).r;
//            o.Albedo *= smoothstep(0.25,.75,flowerAo);
            
            o.Albedo = lerp(o.Albedo, _FlowerColor, flowerAlpha);
	
            	float forestMask = tex2D(_ForestMask, mul(_ForestMask_WorldToLocal, float4(IN.worldPos.x, 0, IN.worldPos.z, 1)).xz).r;
            	o.Albedo = lerp(o.Albedo, _Forest, forestMask);


	
            	//fixed4 spots = tex2D(_SpotMask, TRANSFORM_TEX( IN.worldPos.xz, _SpotMask));
            	//o.Albedo *= lerp(1, _SpotColor, 1-spots.r);

                //o.Albedo = lerp(o.Albedo, tint(o.Albedo, 0, 1.1, .5), 1 - inputOcclusion);
                
                //o.Albedo = _Grass;
                
                /*float tileMaskDistance = 1;			
				float2 nearestTile = round(IN.worldPos.xz);
				for (int x = -1; x <= 1; x++)
				for (int y = -1; y <= 1; y++) {
					float2 pos = nearestTile + float2(x, y);
					float selected = tex2D(_TileMask, mul(_TileMask_WorldToLocal, float4(pos.x, 0, pos.y, 1)).xz).r;
					if (selected > .5)
						 tileMaskDistance = min(tileMaskDistance, sdfBox(IN.worldPos.xz - pos, 0.5));
				}*/
	
				//o.Albedo = saturate(tileMaskDistance);
				
				/*float3 tileMaskEmission = 0;
				tileMaskEmission += _Emissive * smoothstep(0.05, -.025, tileMaskDistance);
				tileMaskEmission += 3.3*_Emissive * smoothstep(0.025, 0.0125, abs(tileMaskDistance - .025));*/
				
				//o.Emission = tileMaskEmission ;
				//o.Albedo  = lerp(o.Albedo, o.Emission, (o.Emission.r + o.Emission.g + o.Emission.b) / 1);

            	/*float noise3 = ClassicNoise(IN.worldPos/4);
        	noise3 += ClassicNoise(IN.worldPos/2+1.24)/2;
			noise3 += ClassicNoise(IN.worldPos+7.54)/4;
        	noise3 += ClassicNoise(IN.worldPos*2+9.456654)/8;

        	noise3 *= 1.5;
        	        	
        	float3 color2 = RGBtoHSV(o.Albedo);
        	color2.z = lerp(color2.z, color2.z / 2, saturate(noise3)); //= max(1, 5 * noise3);
        	color2.y = lerp(color2.y, color2.y * 1.125, saturate(noise3)); //= max(1, 5 * noise3);

        	o.Albedo = HSVtoRGB(color2);*/


            	float2 uv2 = mul(_TileMask_WorldToLocal, round(float4(IN.worldPos.xyz, 1))).xz;
                            			float tileMask = saturate(tex2D(_TileMask, uv2).r);
                            			if (uv2.x < 0 || uv2.x > 1 || uv2.y < 0 || uv2.y > 1)
                            				tileMask = 0;

            	float2 cell2 = round(IN.worldPos.xz);
        	float2 distanceToCell = length( cell2 - IN.worldPos.xz);
        	float circle = tileMask*smoothstep(0.05, 0.025, distanceToCell);
        	o.Albedo *= lerp(1, float3(0,.75,1), saturate(tileMask));
        	o.Emission = ( circle*1.5) * float3(0,1,0); 
            	

            	float tint = smoothstep(_DeepSeaLevel - _DeepSeaSharpness,  _DeepSeaLevel + _DeepSeaSharpness , IN.worldPos.y);
            	//o.Albedo = lerp(o.Albedo, _DeepSeaColor, tint);
//o.Albedo=erosion;
            	
            	
            }
            ENDCG
    }
    }