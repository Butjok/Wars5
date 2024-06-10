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
        _YellowGrass ("_YellowGrass", 2D) = "white" {}
        _Grid ("_Grid", 2D) = "black" {}

        _ForestMask ("_ForestMask", 2D) = "black" {}
        _ForestColor ("_ForestColor", Color) = (1,1,1,1)




        _StonesNormal ("_StonesNormal", 2D) = "bump" {}
        _StonesAlpha ("_StonesAlpha", 2D) = "black" {}

        
        
        _FlowersAlpha ("_FlowersAlpha", 2D) = "black" {}





        _GrassColor ("_GrassColor", Color) = (1,1,1,1)
        _StoneColor ("_StoneColor", Color) = (1,1,1,1)
        _StoneDarkColor ("_StoneDarkColor", Color) = (1,1,1,1)
        _StoneLightColor ("_StoneLightColor", Color) = (1,1,1,1)
        _StoneWheatColor ("_StoneWheatColor", Color) = (1,1,1,1)
        _StoneSandColor ("_StoneWheatColor", Color) = (1,1,1,1)
        _FlowerColor ("_FlowerColor", Color) = (1,1,1,1)

        _Splat2 ("_Splat2", 2D) = "black" {}

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


        _GrassHSVTweak ("_GrassHSVTweak", Vector) = (0,0,0,0)
        _DarkGrassHSVTweak ("_DarkGrassHSVTweak", Vector) = (0,0,0,0)
        _YellowGrassHSVTweak ("_YellowGrassHSVTweak", Vector) = (0,0,0,0)

        _SeaHSVTweak ("_SeaHSVTweak", Vector) = (0,1,1,0)
        _GridColor ("_GridColor", Color) = (1,1,1,1)
        _SandHSVTweak ("_SandHSVTweak", Vector) = (0,1,1,0)

        _SeaStoneColor ("_SeaStoneColor", Color) = (1,1,1,1)
        
        [Toggle(FLAT_COLORS)] _FlatColors ("FLAT_COLORS", Float) = 0
        
        _FlatColors_Grass ("_FlatColors_Grass", Color) = (1,1,1,1)
        _FlatColors_Sea ("_FlatColors_Sea", Color) = (1,1,1,1)
        _FlatColors_Beach ("_FlatColors_Beach", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "BW"="TrueProbes"
        }
        Cull off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        #pragma shader_feature FLAT_COLORS

        #include "Assets/Shaders/Utils.cginc"

        float4 _SeaHSVTweak;
        sampler2D _Grass, _Grid, _DarkGreen, _YellowGrass, _GrassTinted, _GrassTint;
        sampler2D _Normal;
        sampler2D _StonesNormal, _StonesAlpha;
        sampler2D _FlowersAlpha, _Splat2;
        sampler2D _ForestMask;
        float4x4 _ForestMask_WorldToLocal;
        float3 _StoneColor, _GrassColor, _StoneDarkColor, _StoneLightColor, _StoneWheatColor, _StoneSandColor;
        float3 _SpotGrassColor, _SpotOceanColor, _ForestColor;

        float4 _GrassHSVTweak, _DarkGrassHSVTweak, _YellowGrassHSVTweak;

        float _NormalPower;
        half _DeepSeaSharpness;
        float3 _FlowerColor;


        float _DeepSea4Level, _DeepSea4Sharpness, _DeepSea4Thickness;
        float4 _DeepSea4Color;
        float4 _GridColor;

        struct Input {
            float3 worldPos;
            float2 uv_MainTex;
            float2 uv_StonesNormal;
            float2 uv_FlowersAlpha;
            float2 uv_Normal;
        };

        fixed4 _Color;
        fixed4 _SeaColor, _DeepSeaColor, _DeepSeaColor2, _DeepSeaColor3;
        half _SandThickness, _SandSharpness, _SandLevel;

        half _DeepSeaThickness, _DeepSeaLevel, _DeepSea2Level, _DeepSea2Sharpness, _DeepSea3Level, _DeepSea3Sharpness;

        #include "Assets/Shaders/SDF.cginc"
        #include "Assets/Shaders/ClassicNoise.cginc"

        float4 _Grass_ST, _DarkGreen_ST, _YellowGrass_ST, _GrassTint_ST;

        float _SeaLevel, _SeaThickness, _SeaSharpness;
        float4 _SandColor;
        float4 _SandColor2;

        float _SandNoiseScale, _SandNoiseAmplitude;

        fixed4x4 _Splat_WorldToLocal;
        float4 _SandHSVTweak;
        float4 _Normal_ST;
        float3 _SeaStoneColor;


        sampler2D _Caustics;
        int _CausticsColumns, _CausticsRows;
        float _CausticsSpeed;
        float3 _CausticsColor;

        float _CausticsLevel, _CausticsSharpness;

        #if FLAT_COLORS
        float3 _FlatColors_Grass;
        float3 _FlatColors_Sea;
        float3 _FlatColors_Beach;
        #endif

        void surf(Input IN, inout SurfaceOutputStandard o) {

            float2 position = IN.worldPos.xz;
            half sea = smoothstep(_SeaLevel - _SeaSharpness, _SeaLevel + _SeaSharpness, IN.worldPos.y);



            half2 splatUV = mul(_Splat_WorldToLocal, float4(IN.worldPos, 1)).xz;
            half4 splat = tex2D(_Splat2, splatUV);
            half darkGrassIntensity = splat.r;
            half yellowGrassIntensity = splat.b;


            // Albedo comes from a texture tinted by color
            fixed4 grass = tex2D(_Grass, TRANSFORM_TEX(position, _Grass));
            fixed4 grassTinted = tex2D(_GrassTinted, TRANSFORM_TEX(position, _Grass));
            fixed4 grassTint = tex2D(_GrassTint, TRANSFORM_TEX(position, _GrassTint));

            //o.Albedo =  lerp(grass,grassTinted,grassTint) ;
            float3 grassHSV = RGBtoHSV(lerp(grass, grassTinted, grassTint / 2));
            grassHSV.x += _GrassHSVTweak.x;
            grassHSV.y *= _GrassHSVTweak.y;
            grassHSV.z *= _GrassHSVTweak.z;
            o.Albedo = HSVtoRGB(grassHSV);

            float2 darkGreenUv = position;
            darkGreenUv.x += sin(darkGreenUv.y * 2) / 16 + sin(darkGreenUv.y * 5 + .4) / 32 + sin(darkGreenUv.y * 10 + .846) / 32;
            fixed3 darkGrass = tex2D(_DarkGreen, TRANSFORM_TEX(darkGreenUv, _DarkGreen)); //tex2D (_Grass, TRANSFORM_TEX(position, _Grass) );
            float3 darkGrassHSV = RGBtoHSV(darkGrass);
            darkGrassHSV.x += _DarkGrassHSVTweak.x;
            darkGrassHSV.y *= _DarkGrassHSVTweak.y;
            darkGrassHSV.z *= _DarkGrassHSVTweak.z;
            darkGrass = HSVtoRGB(darkGrassHSV);


            o.Albedo = lerp(o.Albedo, darkGrass, darkGrassIntensity);

            float2 wheatUv = position;
            wheatUv.xy += sin(wheatUv.x) / 32 + sin(wheatUv.x * 2.5 + .4) / 64 + sin(wheatUv.x * 10 + .846) / 32;


            fixed3 yellowGrass = tex2D(_YellowGrass, TRANSFORM_TEX(position, _YellowGrass));
            float3 yellowGrassHSV = RGBtoHSV(yellowGrass);
            yellowGrassHSV.x += _YellowGrassHSVTweak.x;
            yellowGrassHSV.y *= _YellowGrassHSVTweak.y;
            yellowGrassHSV.z *= _YellowGrassHSVTweak.z;
            yellowGrass = HSVtoRGB(yellowGrassHSV);
            o.Albedo = lerp(o.Albedo, yellowGrass, yellowGrassIntensity);


            float3 albedoHSV2 = RGBtoHSV(o.Albedo);
            albedoHSV2.y *= 1.05;
            albedoHSV2.z *= .75;
            o.Albedo = lerp(o.Albedo, HSVtoRGB(albedoHSV2), grassTint);



            
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = 1;



            

            float3 normal = UnpackNormal(tex2D(_Normal, TRANSFORM_TEX(position, _Normal)));
            //Normal = normal;
            o.Normal = sign(normal) * pow(abs(normal), _NormalPower);
            o.Normal = lerp(float3(0, 0, 1), o.Normal, sea);



            

            float2 stonesUv = IN.worldPos.xz * .2;

            float3 stoneNormal = UnpackNormal(tex2D(_StonesNormal, stonesUv));
            stoneNormal = sign(stoneNormal) * pow(abs(stoneNormal), .75);
            stoneNormal = normalize(stoneNormal);

            float stoneAlpha = tex2D(_StonesAlpha, stonesUv).r;

            float3 stoneColor = _StoneColor;
            stoneColor = lerp(stoneColor, _StoneDarkColor, darkGrassIntensity);
            stoneColor = lerp(stoneColor, _StoneLightColor, yellowGrassIntensity);
            stoneColor = lerp(stoneColor, _SeaStoneColor, sea);
            o.Albedo = lerp(o.Albedo, stoneColor, stoneAlpha);


            o.Normal = normalize(lerp(o.Normal, stoneNormal, stoneAlpha));


            float flowerAlpha = tex2D(_FlowersAlpha, IN.worldPos.xz * 0.25).r;
            o.Albedo = lerp(o.Albedo, _FlowerColor, flowerAlpha);


            float forestMask = tex2D(_ForestMask, mul(_ForestMask_WorldToLocal, float4(IN.worldPos, 1)).xz).r;
            o.Albedo *= lerp(1, _ForestColor, forestMask);


            half3 seaColor = lerp(_SeaColor, _DeepSeaColor, smoothstep(_DeepSeaLevel - _DeepSeaSharpness, _DeepSeaLevel + _DeepSeaSharpness, IN.worldPos.y));
            seaColor = lerp(seaColor, _DeepSeaColor2, smoothstep(_DeepSea2Level - _DeepSea2Sharpness, _DeepSea2Level + _DeepSea2Sharpness, IN.worldPos.y));
            half deepSea3 = smoothstep(_DeepSea3Level - _DeepSea3Sharpness, _DeepSea3Level + _DeepSea3Sharpness, IN.worldPos.y);
            seaColor = lerp(seaColor, _DeepSeaColor3, deepSea3);
            seaColor = lerp(seaColor, _DeepSea4Color, smoothstep(_DeepSea4Level - _DeepSea4Sharpness, _DeepSea4Level + _DeepSea4Sharpness, IN.worldPos.y));


            o.Albedo = lerp(o.Albedo, seaColor, sea);
            {
                float3 hsv = RGBtoHSV(o.Albedo);
                hsv.x += _SeaHSVTweak.x;
                hsv.y *= _SeaHSVTweak.y;
                hsv.z *= _SeaHSVTweak.z;
                o.Albedo = lerp(o.Albedo, HSVtoRGB(hsv), grassTint * sea);
            }


            float3 sandColor = lerp(_SandColor, _SandColor2, smoothstep(_SandLevel - _SandSharpness, _SandLevel + _SandSharpness, IN.worldPos.y));
            {
                float3 hsv = RGBtoHSV(sandColor);
                hsv.x += _SandHSVTweak.x;
                hsv.y *= _SandHSVTweak.y;
                hsv.z *= _SandHSVTweak.z;
                sandColor = lerp(sandColor, HSVtoRGB(hsv), grassTint);
            }
            float sand = smoothstep(_SandThickness - _SandSharpness, _SandThickness + _SandSharpness, abs(IN.worldPos.y + /*noise/50*/ - _SandLevel));
            o.Albedo = lerp(o.Albedo, sandColor, sand);
            o.Albedo = lerp(o.Albedo, _StoneSandColor, sand * stoneAlpha);

            o.Albedo = lerp(o.Albedo, o.Albedo * _SeaStoneColor, sea * stoneAlpha * (1 - sand));



            
            #if FLAT_COLORS

            o.Albedo = _FlatColors_Grass;
            o.Albedo = lerp(o.Albedo, _FlatColors_Sea, sea);
            o.Albedo = lerp(o.Albedo, _FlatColors_Beach, sand);
            
            #endif

                

            o.Smoothness = 0;
            o.Smoothness = lerp(o.Smoothness, 0, sea);

            float grid = tex2D(_Grid, position - .5).r;
            grid *= 1 - sea;
            float3 gridColor;
            #if FLAT_COLORS
            gridColor = float3(1, 1, 1)*.75;
            #else
            gridColor = _GridColor;
            #endif
            o.Albedo *= lerp(1, gridColor, grid);

            {
                int length = _CausticsColumns * _CausticsRows;
                int frame = round(_CausticsSpeed * _Time.y) % length;
                int column = frame % _CausticsColumns;
                int row = frame / _CausticsColumns;
                float2 offset = float2(1.0 / _CausticsColumns, 1.0 / _CausticsRows);
                float2 uv = frac(IN.worldPos.xz) * offset;
                uv += offset * float2(column, _CausticsRows - row - 1);
                float intensity = smoothstep(_CausticsLevel - _CausticsSharpness, _CausticsLevel + _CausticsSharpness, IN.worldPos.y);
                float caustics = tex2Dlod(_Caustics, float4(uv, 0, lerp(5, 0, intensity))).r;
                o.Emission += caustics * _CausticsColor * sea * intensity;
            }

            o.Albedo *= _Color;


        }
        ENDCG
    }
    FallBack "Diffuse"
}