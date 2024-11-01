Shader "Custom/Terrain"
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
        _OceanMask ("_OceanMask", 2D) = "white" {}
        _Ocean ("_Ocean", 2D) = "white" {}
        _Grid ("_Grid", 2D) = "black" {}
        _Splat ("_Splat", 2D) = "black" {}
_Distance ("_Distance", 2D) = "black" {}
        
        
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

        sampler2D _Grass,_Grid,_Splat,_DarkGreen,_Wheat,_YellowGrass,_Ocean,_OceanMask,_GrassTinted,_GrassTint,_Distance;
        sampler2D _Normal;
        float4 _Normal_ST;

float _LineDistance;
float _LineThickness,_BorderPower;
float _BorderOffset, _BorderThinkness, _BorderSharpness,_OutsideSmoothness,_OutsideOffset,_OutsideIntensity;
float3 _OutsideColor,_BorderColor;
float4 _Bounds;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic,_Radius,_Rounding,_K,_SelectTime,_TimeDirection;
        fixed4 _Color;

        #define SIZE 128
        int2 _From;
        int _Size=3;

        #include "Assets/Shaders/SDF.cginc"
        
        float4 _Grass_ST, _DarkGreen_ST, _Wheat_ST,_YellowGrass_ST,_Ocean_ST,_OceanMask_ST,_GrassTint_ST;
        
        sampler2D _FogOfWar;
        float4x4 _FogOfWar_WorldToLocal;
               
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
            
            half3 splat = tex2D (_Splat, IN.uv_MainTex);
            half darkGrassIntensity = splat.r;
            half wheatIntensity = splat.g;
            half yellowGrassIntensity = splat.b;
            half oceanMask = tex2D (_OceanMask, IN.uv_MainTex);
            
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
            fixed3 finalWheat = wheat;

            

            fixed3 yellowGrass = tex2D (_YellowGrass, TRANSFORM_TEX(position, _YellowGrass) );
            o.Albedo =  lerp(o.Albedo, yellowGrass, yellowGrassIntensity);


            //o.Albedo = lerp(o.Albedo, finalWheat, wheatIntensity);

            float3 ocean = tex2D (_Ocean, IN.uv_MainTex);
            o.Albedo=lerp(o.Albedo, ocean ,1-oceanMask);
            
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

float border2 = smoothstep(_BorderThinkness+_BorderSharpness,_BorderThinkness-_BorderSharpness,abs(dist - _BorderOffset));
float outside = smoothstep(0, _OutsideSmoothness, dist - _OutsideOffset);

//o.Emission = border2*_BorderColor;
o.Albedo *= lerp(float3(1,1,1), _OutsideColor, outside * _OutsideIntensity);
//o.Albedo *= (1-majorLines);
o.Albedo = pow(o.Albedo, lerp(1, _BorderPower, border2));
o.Albedo *= lerp(float3(1,1,1), _BorderColor, border2);


        }
        ENDCG
    }
    FallBack "Diffuse"
}
