Shader "Custom/Terrain4"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        
        
        
        _Normal ("_Normal", 2D) = "bump" {}
        _Grass ("_Grass", 2D) = "white" {}
        _GrassTinted ("_GrassTinted", 2D) = "white" {}
        _GrassTint ("_GrassTint", 2D) = "white" {}
        _DarkGreen ("_DarkGreen", 2D) = "white" {}
        _Wheat ("_Wheat", 2D) = "white" {}
        _WheatTinted ("_WheatTinted", 2D) = "white" {}
        _YellowGrass ("_YellowGrass", 2D) = "white" {}
        _Ocean ("_Ocean", 2D) = "white" {}
        _Grid ("_Grid", 2D) = "black" {}
        _Splat ("_Splat", 2D) = "black" {}
_Distance ("_Distance", 2D) = "black" {}
    	
    	_ForestMask ("_ForestMask", 2D) = "black" {}
        
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Size ("_Size", Range(0,64)) = 0.0
        _Radius ("_Radius", Range(0,10)) = 0.0
        _K ("_K", Range(0,10)) = 0.0
        _Rounding ("_Rounding", Range(-1,1)) = 0.0


_LineDistance("Mayor Line Distance", Range(0, 2)) = 1
_LineThickness("Mayor Line Thickness", Range(0, 0.1)) = 0.05
_BorderOffset("_BorderOffset", Range(-2, 2)) = 1
_BorderPower("_BorderPower", Range(-2, 2)) = 1
_BorderThinkness("_BorderThinkness", Range(-2, 2)) = 1
_BorderSharpness("_BorderSharpness", Range(-2, 2)) = 1
_OutsideSmoothness("_OutsideSmoothness", Range(-2, 2)) = 1
_OutsideOffset("_OutsideOffset", Range(-2, 2)) = 1

_OutsideColor ("_OutsideColor", Color) = (1,1,1,1)
[HDR] _BorderColor ("_BorderColor", Color) = (1,1,1,1)

_OutsideIntensity ("_OutsideIntensity", Range(0,1)) = 0.0





		_SplatScale ("_SplatScale", Float) = 1
        
        _StonesNormal ("_StonesNormal", 2D) = "bump" {}
        _StonesAlpha ("_StonesAlpha", 2D) = "black" {}
        _StonesAo ("_StonesAo", 2D) = "white" {}
        
        _FlowersDiffuse ("_FlowersDiffuse", 2D) = "white" {}
        _FlowersAlpha ("_FlowersAlpha", 2D) = "black" {}
        _FlowersAo ("_FlowersAo", 2D) = "white" {}
        
        
        
        

        _GrassColor ("_GrassColor", Color) = (1,1,1,1)
        _StoneColor ("_StoneColor", Color) = (1,1,1,1)
        _StoneDarkColor ("_StoneDarkColor", Color) = (1,1,1,1)
        _StoneLightColor ("_StoneLightColor", Color) = (1,1,1,1)
        _StoneWheatColor ("_StoneWheatColor", Color) = (1,1,1,1)
        _FlowerColor ("_FlowerColor", Color) = (1,1,1,1)

        _Splat2 ("_Splat2", 2D) = "black" {}
        _Splat2Size ("_Splat2Size", Vector) = (1,1,1,1)
        
        _SeaColor ("_SeaColor", Color) = (1,1,1,1)
        _DeepSeaColor ("_DeepSeaColor", Color) = (1,1,1,1)
        
        _SeaLevel ("_SeaLevel", Float) = 0
        _SeaThickness ("_SeaThickness", Float) = 0.1
        _DeepSeaLevel ("_DeepSeaLevel", Float) = 0
    	_DeepSeaSharpness ("_DeepSeaSharpness", Float) = 0
                _DeepSeaThickness ("_DeepSeaThickness", Float) = 0.1
        _SeaSharpness ("_SeaSharpness", Float) = 0.1
        
        _SandColor ("_SandColor", Color) = (1,1,1,1)
        _SandLevel ("_SandLevel",Float)=1
        _SandThickness ("_SandThickness",Float)=1
        _SandSharpness ("_SandSharpness",Float)=1
        
        _SandNoiseScale ("_SandNoiseScale",Float)=1
        _SandNoiseAmplitude ("_SandNoiseAmplitude",Float)=1
        
        [HDR]_Emissive ("_Emissive", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "BW"="TrueProbes" }
        Cull off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        #if defined(SHADER_API_D3D11)
        //#pragma require interpolators32
        #endif

        #include "Assets/Shaders/Utils.cginc"

        sampler2D _Grass,_Grid,_Splat,_DarkGreen,_Wheat,_YellowGrass,_GrassTinted,_GrassTint,_Distance;
        sampler2D _Normal;
        sampler2D _StonesNormal,_StonesAlpha, _StonesAo;
        sampler2D _FlowersDiffuse,_FlowersAlpha, _FlowersAo, _Splat2;
        sampler2D _ForestMask;
        float4x4 _ForestMask_WorldToLocal;
        float3 _StoneColor, _GrassColor, _StoneDarkColor, _StoneLightColor, _StoneWheatColor;
        float4 _Normal_ST, _Emissive;

float _LineDistance;
float _LineThickness,_BorderPower;
float _BorderOffset, _BorderThinkness, _BorderSharpness,_OutsideSmoothness,_OutsideOffset,_OutsideIntensity;
        half _DeepSeaSharpness;
float3 _OutsideColor,_BorderColor,_FlowerColor;
float4 _Bounds;

float4 _MapSize;
float2 _Splat2Size;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
            float2 uv_StonesNormal;
            float2 uv_FlowersAlpha;
            float2 uv_Splat2;
        };

        half _Glossiness;
        half _Metallic,_Radius,_Rounding,_K,_SelectTime,_TimeDirection;
        half _SplatScale;
        fixed4 _Color;
        fixed4 _SeaColor,_DeepSeaColor;
        half _SandThickness, _SandSharpness, _SandLevel;
        
        half _DeepSeaThickness, _DeepSeaLevel;

        #define SIZE 128
        int2 _From;
        int _Size=3;

        #include "Assets/Shaders/SDF.cginc"
        #include "Assets/Shaders/ClassicNoise.cginc"
        
        float4 _Grass_ST, _DarkGreen_ST, _Wheat_ST,_YellowGrass_ST,_Ocean_ST,_OceanMask_ST,_GrassTint_ST;
        
        sampler2D _FogOfWar;
        float4x4 _FogOfWar_WorldToLocal;

        float _SeaLevel, _SeaThickness, _SeaSharpness;
        float4 _SandColor;

        float _SandNoiseScale, _SandNoiseAmplitude;
        sampler2D _TileMask;
        fixed4x4 _TileMask_WorldToLocal;

        fixed4x4 _Splat_WorldToLocal;
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            float2 position = IN.worldPos.xz;
                        int2 cell = round(position);
            
                        half minDist = 999;
            
                        half radiusDistance = length(_From-position)-(_Time.y - _SelectTime)*50*_TimeDirection;
                        //minDist = max(minDist,radiusDistance);
                        
                        half highlightIntensity = smoothstep(.01,0.005,minDist-_Rounding);
                        half border = smoothstep(.01 + .01,0.01,abs(0.0-(minDist-_Rounding)));
                        half3 highlight = half3(1,1,1)/3;
                        
                        half radius = smoothstep(_Radius+1,_Radius, radiusDistance);
                        
                        //half4 splat = tex2D (_Splat, IN.uv_MainTex);
        				half2 splatUV = mul(_Splat_WorldToLocal, float4(IN.worldPos,1)).xz;
        				half4 splat = tex2D(_Splat2, splatUV);
                        half darkGrassIntensity = splat.r;
                        half wheatIntensity = splat.a;
                        half yellowGrassIntensity = splat.b;
                        
                        // Albedo comes from a texture tinted by color
                        fixed4 grass = tex2D (_Grass, TRANSFORM_TEX(position, _Grass) );
                        fixed4 grassTinted = tex2D (_GrassTinted, TRANSFORM_TEX(position, _Grass) );
                        fixed4 grassTint = tex2D (_GrassTint, TRANSFORM_TEX(position, _GrassTint) );
                        
                        o.Albedo =  lerp(grass,grassTinted,grassTint) ;
                        
                        float2 darkGreenUv = position;
                        darkGreenUv.x += sin(darkGreenUv.y*2)/16 + sin(darkGreenUv.y*5+.4)/32  + sin(darkGreenUv.y*10+.846)/32;
                        fixed3 darkGrass = tex2D (_DarkGreen, TRANSFORM_TEX(darkGreenUv, _DarkGreen) );//tex2D (_Grass, TRANSFORM_TEX(position, _Grass) );
            
                        
                        o.Albedo =  lerp(o.Albedo, darkGrass, darkGrassIntensity);
            
                        float2 wheatUv = position;
                        wheatUv.xy += sin(wheatUv.x)/32 + sin(wheatUv.x*2.5+.4)/64  + sin(wheatUv.x*10+.846)/32;
                        fixed3 wheat = tex2D (_Wheat, TRANSFORM_TEX(wheatUv, _Wheat) );;
                        //fixed3 wheatTinted = tex2D (_WheatTinted, TRANSFORM_TEX(wheatUv, _Wheat) );;
                        //fixed3 finalWheat = lerp(wheat,wheatTinted,grassTint);
                        fixed3 finalWheat =wheat;// Tint( wheat, 0, 1 - .025/2, 1 );
            
                        
            
                        fixed3 yellowGrass = tex2D (_YellowGrass, TRANSFORM_TEX(position, _YellowGrass) );
                        o.Albedo =  lerp(o.Albedo, yellowGrass, yellowGrassIntensity);
            
            
                        //o.Albedo = lerp(o.Albedo, finalWheat, wheatIntensity);
            
                        //float3 ocean = tex2D (_Ocean, IN.uv_MainTex);
                        //o.Albedo=lerp(o.Albedo, ocean ,1-oceanMask);
                        
                        // Metallic and smoothness come from slider variables
                        o.Metallic = 0;
                        o.Smoothness = _Glossiness;
                        o.Alpha = 1;
            
                        //o.Albedo=float3(1,0,0);
                        
                       // o.Emission=border*10*o.Albedo+highlightIntensity*o.Albedo * tex2D (_Grid, position-.5) *10;
                       // o.Emission*= radius;
                        
                       // o.Emission *=  IN.worldPos.y > 0;
            
                        float3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(position, _Normal) ));
                        normal = sign(normal) * pow(abs(normal),.75);
                        //normal.z/=2;
                        normal=normalize(normal);
                        o.Normal = normal;
            
                        //o.Albedo=splat;
            
                        half gridMask = smoothstep(-.1,-.05,IN.worldPos.y);
                        o.Albedo *= lerp(float3(1,1,1), 1-tex2D (_Grid, position-.5), gridMask);
            
                        //o.Albedo = tint(o.Albedo, .0, .975, 1);
            
            float2 uv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw - _Bounds.xy);
            fixed4 c = tex2D (_Distance, uv);
            float dist = c.r;
            float distanceChange = fwidth(dist) * 0.5;
            float majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
            float majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);

			float2 stonesUv = IN.worldPos.xz * .12;
        	
            float3 stoneNormal = UnpackNormal( tex2D(_StonesNormal, stonesUv) );
            stoneNormal = sign(stoneNormal) * pow(abs(stoneNormal),.75);
            stoneNormal=normalize(stoneNormal);
            
            float stoneAlpha = tex2D(_StonesAlpha,stonesUv).r;

			float3 stoneColor = _StoneColor;
			stoneColor = lerp(stoneColor, _StoneDarkColor, darkGrassIntensity);
			stoneColor = lerp(stoneColor, _StoneLightColor, yellowGrassIntensity);
			//stoneColor = lerp(stoneColor, _StoneWheatColor, wheatIntensity);
            o.Albedo = lerp(o.Albedo, stoneColor, stoneAlpha);
            
            o.Smoothness=lerp(o.Smoothness, .0, stoneAlpha);
            o.Normal = normalize(lerp(o.Normal, stoneNormal, stoneAlpha));

            o.Occlusion = min(o.Occlusion, tex2D(_StonesAo, stonesUv).r);

            float flowerAlpha = tex2D(_FlowersAlpha, IN.worldPos.xz * 0.125).r;

//half flowerAo = tex2D(_FlowersAo, IN.uv_FlowersAlpha).r;
//            o.Albedo *= smoothstep(0.25,.75,flowerAo);
            
            o.Albedo = lerp(o.Albedo, _FlowerColor, flowerAlpha);
            
            //o.Occlusion = min(o.Occlusion, flowerAo);







float forestMask = tex2D(_ForestMask, mul(_ForestMask_WorldToLocal, float4(IN.worldPos, 1)).xz).r;
        	float3 forestHSV = RGBtoHSV(o.Albedo);
        	forestHSV.y *= 1.25; // saturation
        	forestHSV.z *= .5; // value
        	float3 forest = HSVtoRGB(forestHSV);
        	o.Albedo = lerp(o.Albedo, forest, forestMask);




        	
            


            float noise = 0;
            noise += ClassicNoise(float3(IN.worldPos.xz, 0) * _SandNoiseScale);
            noise += ClassicNoise(float3(IN.worldPos.xz*2, 0) * _SandNoiseScale) ;
            noise /= 2;
            
            float deepSeaNoise = 0;
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/5, 0));
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/5*2, 0))/2;
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/5*4, 0))/4;
            deepSeaNoise /= 1.25;
            


            o.Albedo = lerp(o.Albedo, _SandColor, smoothstep(_SandThickness - _SandSharpness, _SandThickness + _SandSharpness ,abs(IN.worldPos.y+ noise/50 - _SandLevel)));
            o.Albedo = lerp(o.Albedo, lerp(_SeaColor, _DeepSeaColor, smoothstep(_DeepSeaLevel - _DeepSeaSharpness, _DeepSeaLevel + _DeepSeaSharpness, (deepSeaNoise+noise/5)*1.125+.33)), smoothstep(_SeaLevel - _SeaSharpness, _SeaLevel + _SeaSharpness, IN.worldPos.y));//smoothstep(_SeaLevel - _SeaSharpness, _SeaLevel + _SeaSharpness ,IN.worldPos.y));
            
            
            float seaLevel = IN.worldPos.y - _SeaLevel;
            seaLevel += noise * _SandNoiseAmplitude;
            
            float seaMask1 = smoothstep(_SeaThickness + _SeaSharpness, _SeaThickness - _SeaSharpness, seaLevel);
            float seaMask2 = smoothstep(_SeaThickness + _SeaSharpness*20, _SeaThickness - _SeaSharpness*20, -seaLevel);
            //o.Albedo = min(seaMask1,seaMask2);
            
            float sandMask = min(seaMask1,seaMask2);//smoothstep(_SeaThickness + _SeaSharpness, _SeaThickness - _SeaSharpness, abs(IN.worldPos.y - _SeaLevel));
            //o.Albedo = lerp(o.Albedo, _SandColor, sandMask);
            
            
            
			//float tileMask = tex2D(_TileMask, mul(_TileMask_WorldToLocal, float4(IN.worldPos.x, 0, IN.worldPos.z, 1)).xz).r;
			//if (uv2.x < 0 || uv2.x > 1 || uv2.y < 0 || uv2.y > 1)
			//	tileMask = 0;
			
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
