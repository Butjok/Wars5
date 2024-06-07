Shader "Custom/Terrain4"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    	
    	_Caustics ("_Caustics", 2D) = "black" {}
    	_CausticsColumns ("_CausticsColumns", Integer) = 4
    	_CausticsRows ("_CausticsRows", Integer) = 8
    	_CausticsSpeed ("_CausticsSpeed", Float) = 25
    	_CausticsColor ("_CausticsColor", Color) = (1,1,1,1)
    	_CausticsLevel ("_CausticsLevel", Float) = 0
    	_CausticsSharpness ("_CausticsSharpness", Float) = 0
        
        
        
        _Normal ("_Normal", 2D) = "bump" {}
    	_NormalPower ("_NormalPower", Float) = 0.5
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
    	_ForestColor ("_ForestColor", Color) = (1,1,1,1)
        
        
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
    	_StoneSandColor ("_StoneWheatColor", Color) = (1,1,1,1)
        _FlowerColor ("_FlowerColor", Color) = (1,1,1,1)

        _Splat2 ("_Splat2", 2D) = "black" {}
        _Splat2Size ("_Splat2Size", Vector) = (1,1,1,1)
        
        _SeaColor ("_SeaColor", Color) = (1,1,1,1)
        _DeepSeaColor ("_DeepSeaColor", Color) = (1,1,1,1)
    	_DeepSeaColor2 ("_DeepSeaColor2", Color) = (1,1,1,1)
    	_DeepSeaColor3 ("_DeepSeaColor3", Color) = (1,1,1,1)
	    _DeepSea4Color ("_DeepSea4Color", Color) = (1,1,1,1)
        
        _SeaLevel ("_SeaLevel", Float) = 0
        _SeaThickness ("_SeaThickness", Float) = 0.1
        _DeepSeaLevel ("_DeepSeaLevel", Float) = 0
    	_DeepSea2Level ("_DeepSea2Level", Float) = 0
    	_DeepSea3Level ("_DeepSea3Level", Float) = 0
    	_DeepSea4Level ("_DeepSea4Level", Float) = 0
    	
    	_DeepSeaSharpness ("_DeepSeaSharpness", Float) = 0
    	_DeepSea2Sharpness ("_DeepSea2Sharpness", Float) = 0
    	_DeepSea3Sharpness ("_DeepSea3Sharpness", Float) = 0
		_DeepSea4Sharpness ("_DeepSea4Sharpness", Float) = 0
    	
                _DeepSeaThickness ("_DeepSeaThickness", Float) = 0.1
        _SeaSharpness ("_SeaSharpness", Float) = 0.1
        
        _SandColor ("_SandColor", Color) = (1,1,1,1)
    	_SandColor2 ("_SandColor2", Color) = (1,1,1,1)
        _SandLevel ("_SandLevel",Float)=1
        _SandThickness ("_SandThickness",Float)=1
        _SandSharpness ("_SandSharpness",Float)=1
        
        _SandNoiseScale ("_SandNoiseScale",Float)=1
        _SandNoiseAmplitude ("_SandNoiseAmplitude",Float)=1
        
        [HDR]_Emissive ("_Emissive", Color) = (0,0,0,1)
    	
    	_SpotMask ("_SpotMask", 2D) = "white" {}
    	_SpotGrassColor ("_SpotGrassColor", Color) = (1,1,1,1)
    	_SpotOceanColor ("_SpotOceanColor", Color) = (1,1,1,1)
    	
    	_Erosion ("_Erosion", 2D) = "white" {}
    	
    	_GrassHSVTweak ("_GrassHSVTweak", Vector) = (0,0,0,0)
    	_DarkGrassHSVTweak ("_DarkGrassHSVTweak", Vector) = (0,0,0,0)
    	_YellowGrassHSVTweak ("_YellowGrassHSVTweak", Vector) = (0,0,0,0)
    	
    	_SeaHSVTweak ("_SeaHSVTweak", Vector) = (0,1,1,0)
    	_GridColor ("_GridColor", Color) = (1,1,1,1)
    	_SandHSVTweak ("_SandHSVTweak", Vector) = (0,1,1,0)
    	
    	_SeaStoneColor ("_SeaStoneColor", Color) = (1,1,1,1)
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
        #pragma target 6.0

        #if defined(SHADER_API_D3D11)
        //#pragma require interpolators32
        #endif

        #include "Assets/Shaders/Utils.cginc"

        float4 _SeaHSVTweak;
        sampler2D _Grass,_Grid,_Splat,_DarkGreen,_Wheat,_YellowGrass,_GrassTinted,_GrassTint,_Distance;
        sampler2D _Normal;
        sampler2D _StonesNormal,_StonesAlpha, _StonesAo;
        sampler2D _FlowersDiffuse,_FlowersAlpha, _FlowersAo, _Splat2;
        sampler2D _ForestMask, _SpotMask;
        sampler2D _Erosion;
        float4x4 _ForestMask_WorldToLocal;
        float3 _StoneColor, _GrassColor, _StoneDarkColor, _StoneLightColor, _StoneWheatColor, _StoneSandColor;
        float4  _Emissive;
        float3 _SpotGrassColor, _SpotOceanColor, _ForestColor;

        float4 _GrassHSVTweak, _DarkGrassHSVTweak, _YellowGrassHSVTweak;

        float _NormalPower;
float _LineDistance;
float _LineThickness,_BorderPower;
float _BorderOffset, _BorderThinkness, _BorderSharpness,_OutsideSmoothness,_OutsideOffset,_OutsideIntensity;
        half _DeepSeaSharpness;
float3 _OutsideColor,_BorderColor,_FlowerColor;
float4 _Bounds;

float4 _MapSize;
float2 _Splat2Size;

        float _DeepSea4Level, _DeepSea4Sharpness, _DeepSea4Thickness;
        float4 _DeepSea4Color;
        float4 _GridColor;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
            float2 uv_StonesNormal;
            float2 uv_FlowersAlpha;
            float2 uv_Splat2;
        	float2 uv_Normal;
        };

        half _Glossiness;
        half _Metallic,_Radius,_Rounding,_K,_SelectTime,_TimeDirection;
        half _SplatScale;
        fixed4 _Color;
        fixed4 _SeaColor,_DeepSeaColor, _DeepSeaColor2, _DeepSeaColor3;
        half _SandThickness, _SandSharpness, _SandLevel;
        
        half _DeepSeaThickness, _DeepSeaLevel, _DeepSea2Level, _DeepSea2Sharpness, _DeepSea3Level, _DeepSea3Sharpness;

        #define SIZE 128
        int2 _From;
        int _Size=3;

        #include "Assets/Shaders/SDF.cginc"
        #include "Assets/Shaders/ClassicNoise.cginc"
        
        float4 _Grass_ST, _DarkGreen_ST, _Wheat_ST,_YellowGrass_ST,_Ocean_ST,_OceanMask_ST,_GrassTint_ST, _SpotMask_ST;
        
        sampler2D _FogOfWar;
        float4x4 _FogOfWar_WorldToLocal;

        float _SeaLevel, _SeaThickness, _SeaSharpness;
        float4 _SandColor;
float4 _SandColor2;

        float _SandNoiseScale, _SandNoiseAmplitude;
        sampler2D _TileMask;
        fixed4x4 _TileMask_WorldToLocal;

        fixed4x4 _Splat_WorldToLocal;
        float4 _SandHSVTweak;
        float4 _Normal_ST;
        float3 _SeaStoneColor;

        float3 Overlay(float3 bg, float3 fg) {
	        return bg < 0.5 ? (2.0 * bg * fg) : (1.0 - 2.0 * (1.0 - bg) * (1.0 - fg));
        }

        float4x4 _Erosion_WorldToLocal;

        sampler2D _Caustics;
        int _CausticsColumns, _CausticsRows;
        float _CausticsSpeed;
        float3 _CausticsColor;

        float _CausticsLevel, _CausticsSharpness;
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
        	

        	float erosion = tex2D(_Erosion, IN.uv_MainTex).r;

            float2 position = IN.worldPos.xz;
        	//fixed4 spots = tex2D(_SpotMask, TRANSFORM_TEX(position, _SpotMask));
        	//fixed spot = 1-spots.r;
        	//IN.worldPos += 0.01 * (spot) * float3(0,1,0);
        	half sea = smoothstep(_SeaLevel - _SeaSharpness,  _SeaLevel + _SeaSharpness , IN.worldPos.y);
        	
            
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
                        
                        //o.Albedo =  lerp(grass,grassTinted,grassTint) ;
        	float3 grassHSV = RGBtoHSV(lerp(grass,grassTinted,grassTint/2));
        	grassHSV.x += _GrassHSVTweak.x;
        	grassHSV.y *=  _GrassHSVTweak.y;
        	grassHSV.z *= _GrassHSVTweak.z;
        	o.Albedo = HSVtoRGB(grassHSV);
                        
                        float2 darkGreenUv = position;
                        darkGreenUv.x += sin(darkGreenUv.y*2)/16 + sin(darkGreenUv.y*5+.4)/32  + sin(darkGreenUv.y*10+.846)/32;
                        fixed3 darkGrass = tex2D (_DarkGreen, TRANSFORM_TEX(darkGreenUv, _DarkGreen) );//tex2D (_Grass, TRANSFORM_TEX(position, _Grass) );
        	float3 darkGrassHSV = RGBtoHSV(darkGrass);
        	darkGrassHSV. x += _DarkGrassHSVTweak.x;
        	darkGrassHSV.y *= _DarkGrassHSVTweak.y;
        	darkGrassHSV.z *= _DarkGrassHSVTweak.z;
        	darkGrass = HSVtoRGB(darkGrassHSV);
            
                        
                        o.Albedo =  lerp(o.Albedo, darkGrass, darkGrassIntensity);
            
                        float2 wheatUv = position;
                        wheatUv.xy += sin(wheatUv.x)/32 + sin(wheatUv.x*2.5+.4)/64  + sin(wheatUv.x*10+.846)/32;
                        fixed3 wheat = tex2D (_Wheat, TRANSFORM_TEX(wheatUv, _Wheat) );;
                        //fixed3 wheatTinted = tex2D (_WheatTinted, TRANSFORM_TEX(wheatUv, _Wheat) );;
                        //fixed3 finalWheat = lerp(wheat,wheatTinted,grassTint);
                        fixed3 finalWheat =wheat;// Tint( wheat, 0, 1 - .025/2, 1 );
            
                        
            
                        fixed3 yellowGrass = tex2D (_YellowGrass, TRANSFORM_TEX(position, _YellowGrass) );
        	float3 yellowGrassHSV = RGBtoHSV(yellowGrass);
			yellowGrassHSV.x += _YellowGrassHSVTweak.x;
			yellowGrassHSV.y *= _YellowGrassHSVTweak.y;
        	 			yellowGrassHSV.z *= _YellowGrassHSVTweak.z;
        	yellowGrass = HSVtoRGB(yellowGrassHSV);
                        o.Albedo =  lerp(o.Albedo, yellowGrass, yellowGrassIntensity);



 	float3 albedoHSV2 = RGBtoHSV(o.Albedo);
        	albedoHSV2.y *= 1.05;
        	albedoHSV2.z *= .75;
        	o.Albedo = lerp (o.Albedo, HSVtoRGB(albedoHSV2), grassTint);





        	//return;

        				//o.Albedo.rgb *= lerp(float3(1,1,1), _SpotGrassColor, spot);
            
            
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
        	//Normal = normal;
        	o.Normal=sign(normal) * pow(abs(normal), _NormalPower);
        	o.Normal = lerp(float3(0,0,1), o.Normal, sea);
                        //normal = sign(normal) * pow(abs(normal),.8);
                        //normal.z/=2;
                        //normal=normalize(normal);
                        //o.Normal = normal;
            
                        //o.Albedo=splat;
            
                        half gridMask = smoothstep(-.1,-.05,IN.worldPos.y);
                        //o.Albedo *= lerp(float3(1,1,1), (1-tex2D (_Grid, position-.5)), gridMask);
            
                        //o.Albedo = tint(o.Albedo, .0, .975, 1);
            
            float2 uv = (IN.worldPos.xz - _Bounds.xy) / (_Bounds.zw - _Bounds.xy);
            fixed4 c = tex2D (_Distance, uv);
            float dist = c.r;
            float distanceChange = fwidth(dist) * 0.5;
            float majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
            float majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);

			float2 stonesUv = IN.worldPos.xz * .2;
        	
            float3 stoneNormal = UnpackNormal( tex2D(_StonesNormal, stonesUv) );
            stoneNormal = sign(stoneNormal) * pow(abs(stoneNormal),.75);
            stoneNormal=normalize(stoneNormal);
            
            float stoneAlpha = tex2D(_StonesAlpha,stonesUv).r;

			float3 stoneColor = _StoneColor;
			stoneColor = lerp(stoneColor, _StoneDarkColor, darkGrassIntensity);
			stoneColor = lerp(stoneColor, _StoneLightColor, yellowGrassIntensity);
			//stoneColor = lerp(stoneColor, _StoneWheatColor, wheatIntensity);
        	stoneColor = lerp(stoneColor, _SeaStoneColor, sea);
            o.Albedo = lerp(o.Albedo, stoneColor, stoneAlpha);
            
            //o.Smoothness=lerp(o.Smoothness, .0, stoneAlpha);
        	o.Smoothness = _Glossiness;
            o.Normal = normalize(lerp(o.Normal, stoneNormal, stoneAlpha));

            //o.Occlusion = min(o.Occlusion, tex2D(_StonesAo, stonesUv).r);

            float flowerAlpha = tex2D(_FlowersAlpha, IN.worldPos.xz * 0.25).r;

//half flowerAo = tex2D(_FlowersAo, IN.uv_FlowersAlpha).r;
//            o.Albedo *= smoothstep(0.25,.75,flowerAo);
            
            o.Albedo = lerp(o.Albedo, _FlowerColor, flowerAlpha);
            
            //o.Occlusion = min(o.Occlusion, flowerAo);







float forestMask = tex2D(_ForestMask, mul(_ForestMask_WorldToLocal, float4(IN.worldPos, 1)).xz).r;
        	/*float3 forestHSV = RGBtoHSV(o.Albedo);
        	forestHSV.y *= 1.25; // saturation
        	forestHSV.z *= .5; // value
        	float3 forest = HSVtoRGB(forestHSV);*/
        	o.Albedo *= lerp(1, _ForestColor, forestMask);
        	//o.Albedo = lerp(o.Albedo, forest, forestMask);








        	/*float noise3 = ClassicNoise(IN.worldPos/4);
        	noise3 += ClassicNoise(IN.worldPos/2+1.24)/2;
			noise3 += ClassicNoise(IN.worldPos+7.54)/4;
        	noise3 += ClassicNoise(IN.worldPos*2+9.456654)/8;

        	noise3 *= 1.5;
        	        	
        	float3 color2 = RGBtoHSV(o.Albedo);
        	color2.z = lerp(color2.z, color2.z / 2, saturate(noise3)); //= max(1, 5 * noise3);
        	color2.y = lerp(color2.y, color2.y * 1.125, saturate(noise3)); //= max(1, 5 * noise3);

        	o.Albedo = HSVtoRGB(color2);*/





        	


        	
            


            /*float noise = 0;
            noise += ClassicNoise(float3(IN.worldPos.xz, 0) * _SandNoiseScale);
            noise += ClassicNoise(float3(IN.worldPos.xz*2, 0) * _SandNoiseScale) ;
            noise /= 2;
            
            float deepSeaNoise = 0;
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/6, 0));
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/5*2, 0))/2;
            deepSeaNoise += ClassicNoise(float3(IN.worldPos.xz/4*4, 0))/4;
            deepSeaNoise /= 1.25;*/
            

        	
        	//_SeaColor.rgb *= lerp (float3(1,1,1), _SpotOceanColor, spot);
        	//_DeepSeaColor.rgb *= lerp (float3(1,1,1), _SpotOceanColor, spot);

        	//o.Albedo *= lerp(float3(1,1,1), _SpotGrassColor, spot);


        	
        	half3 seaColor = lerp(_SeaColor, _DeepSeaColor, smoothstep(_DeepSeaLevel - _DeepSeaSharpness,  _DeepSeaLevel + _DeepSeaSharpness , IN.worldPos.y));
        	seaColor = lerp (seaColor, _DeepSeaColor2, smoothstep( _DeepSea2Level - _DeepSea2Sharpness,  _DeepSea2Level + _DeepSea2Sharpness , IN.worldPos.y));
        	half deepSea3 = smoothstep( _DeepSea3Level - _DeepSea3Sharpness,  _DeepSea3Level + _DeepSea3Sharpness , IN.worldPos.y);
        	seaColor = lerp (seaColor, _DeepSeaColor3,  deepSea3);
        	seaColor = lerp (seaColor, _DeepSea4Color, smoothstep( _DeepSea4Level - _DeepSea4Sharpness,  _DeepSea4Level + _DeepSea4Sharpness , IN.worldPos.y));

        	/*{
        		float3 seaTint = .5;
        		float3 cyan = float3(0,.425,1);
        		if (erosion < .5)
        			seaTint = lerp(float3(0,0,.055), .5, erosion*2);
        		else if (erosion < .75)
        			seaTint = lerp(.5, cyan, (erosion - .5) * 4);
        		else
        			seaTint = cyan;
        		seaColor *= seaTint;
        	}*/
        	//seaColor *= erosion;
        	



        	
        	//seaColor *= lerp (float3(1,1,1), _SpotOceanColor, spot);
            //o.Albedo = lerp(o.Albedo, o.Albedo * seaColor,sea);
        	o.Albedo = lerp(o.Albedo, seaColor, sea);
        	{
        		float3 hsv = RGBtoHSV(o.Albedo);
        		hsv.x += _SeaHSVTweak.x;
        		hsv.y *= _SeaHSVTweak.y;
        		hsv.z *= _SeaHSVTweak.z;
        		o.Albedo = lerp(o.Albedo, HSVtoRGB(hsv), grassTint*sea);
        	}

        	

        	float3 sandColor = lerp(_SandColor, _SandColor2, smoothstep( _SandLevel - _SandSharpness,  _SandLevel + _SandSharpness , IN.worldPos.y));
        	{
        		float3 hsv = RGBtoHSV(sandColor);
        		hsv.x += _SandHSVTweak.x;
        		hsv.y *= _SandHSVTweak.y;
        		hsv.z *= _SandHSVTweak.z;
        		sandColor = lerp(sandColor, HSVtoRGB(hsv), grassTint);
        	}
        	/*{
        	    float3 sandTint = .5;
        		float3 a = float3(.5,.4,.3);
        		if (erosion < .5)
        			sandTint = .5;
        		else if (erosion < .9)
        			sandTint = lerp(.5, a, (erosion - .5) / (.9 - .5));
        		else
        			sandTint = a;
        		sandColor = Overlay(sandColor, sandTint);
        	}*/
        	float sand = smoothstep(_SandThickness - _SandSharpness, _SandThickness + _SandSharpness ,abs(IN.worldPos.y+ /*noise/50*/ - _SandLevel));
            o.Albedo = lerp(o.Albedo, sandColor, sand);
        	o.Albedo = lerp(o.Albedo, _StoneSandColor, sand * stoneAlpha);

        	o.Albedo = lerp(o.Albedo, o.Albedo*_SeaStoneColor, sea*stoneAlpha*(1-sand));
            
            float seaLevel = IN.worldPos.y - _SeaLevel;
            //seaLevel += noise * _SandNoiseAmplitude;
            
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
        	float border2 = smoothstep(0.025, 0.0125, abs(tileMaskDistance - .025));
            tileMaskEmission += smoothstep(0.05, -.025, tileMaskDistance);
			float2 cell2 = round(IN.worldPos.xz);
        	float2 distanceToCell = length( cell2 - IN.worldPos.xz);
        	float circle = tileMaskEmission*smoothstep(0.05, 0.025, distanceToCell);
        	o.Albedo *= lerp(1, float3(0,.75,1), saturate(tileMaskEmission));
        	o.Emission = (border2 + circle*1.5) * float3(0,1,0); 
        	
        	
        	//tileMaskEmission += border2 * 2.5;
            //tileMaskEmission += 5 * smoothstep(0.025, 0.0125, abs(tileMaskDistance - .025));
            
            //o.Emission = tileMaskEmission * _Emissive;
            //o.Albedo  = lerp(o.Albedo, o.Emission, (o.Emission.r + o.Emission.g + o.Emission.b) / 1);

        	//o.Emission = tileMaskEmission * _Emissive;
        	//o.Albedo = lerp(o.Albedo, _Emissive, border2);

        	//o.Emission *= lerp(float3(1,1,1), (1-tex2D (_Grid, position-.5)), gridMask);

        	


        	//o.Albedo = spot;

        	

        	//o.Albedo = noise3;

        	o.Smoothness=0;
        	o.Smoothness= lerp (o.Smoothness, 0, sea);

        	//o.Albedo = lerp (o.Albedo, _DeepSeaColor3, deepSea3*.5);

        	//o.Albedo = erosion;
        	//o.Albedo *= erosion;

        	//o.Albedo =  tex2D(_Erosion, mul(_Erosion_WorldToLocal, float4(IN.worldPos.xyz, 1)).xz).rgb;

        	//o.Albedo = grassTint;

        	//


        	float grid = tex2D( _Grid, position-.5 ).r;
        	grid *= 1-sea;
        	o.Albedo *= lerp(1, _GridColor, grid);

        	{
        		int length = _CausticsColumns * _CausticsRows;
				int frame = round(_CausticsSpeed * _Time.y) % length;
				int column = frame % _CausticsColumns;
				int row = frame / _CausticsColumns;
				float2 offset = float2(1.0 / _CausticsColumns, 1.0 / _CausticsRows);
				float2 uv = frac(IN.worldPos.xz) * offset;
				uv += offset * float2(column, _CausticsRows-row-1);
        		float intensity = smoothstep (_CausticsLevel  - _CausticsSharpness, _CausticsLevel + _CausticsSharpness, IN.worldPos.y);
				float caustics = tex2Dlod(_Caustics, float4(uv,0,lerp(5, 0, intensity))).r;
        		o.Emission += caustics * _CausticsColor * sea * intensity;
        	}

        	o.Albedo *= _Color;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
