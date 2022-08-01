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
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Size ("_Size", Range(0,64)) = 0.0
        _Radius ("_Radius", Range(0,10)) = 0.0
        _K ("_K", Range(0,10)) = 0.0
        _Rounding ("_Rounding", Range(-1,1)) = 0.0
        _Splat ("_Splat", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "BW"="TrueProbes" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _Grass,_Grid,_Splat,_DarkGreen,_Wheat,_YellowGrass,_Ocean,_OceanMask,_GrassTinted,_GrassTint,_WheatTinted;
        sampler2D _Normal;
        float4 _Normal_ST;

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
         int2 _Positions[SIZE] = {
            int2(0,0),
            int2(1,1),
            int2(2,2),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

             int2(999,999),
            int2(999,999),
             int2(999,999),
            int2(999,999),
             int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),

            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999),
            int2(999,999)
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        half sdfBox(half2 p, half2 size)
        {
            half2 d = abs(p) - size;  
	        return length(max(d, half2(0,0))) + min(max(d.x, d.y), 0.0);
        }

// polynomial one, red
float smin0( float a, float b, float k )
{
	float h = clamp( 0.5 + 0.5*(b-a)/k, 0.0, 1.0 );
	return lerp( b, a, h ) - k*h*(1.0-h);
}

// only works on positive numbers,  green
float smin1(float a, float b, float k)
{
    return pow((0.5 * (pow(a, -k) + pow(b, -k))), (-1.0 / k));
}

// has a log2 off when they are equal,  blue
float smin2(float a, float b, float k)
{
    return -log(exp(-k * a) + exp(-k * b)) / k;
}

// works for both positive and negative numbers and no problem when a == b,  purple
float smin3(float a, float b, float k)
{
    float x = exp(-k * a);
    float y = exp(-k * b);
    return (a * x + b * y) / (x + y);
}

////////////////////////////////////////////////////

float smax0(float a, float b, float k)
{
    return smin1(a, b, -k);
}

float smax1(float a, float b, float k)
{
    return log(exp(k * a) + exp(k * b)) / k;
}

float smax2(float a, float b, float k)
{
    return smin3(a, b, -k);
}

        float4 _Grass_ST, _DarkGreen_ST, _Wheat_ST,_YellowGrass_ST,_Ocean_ST,_OceanMask_ST,_GrassTint_ST;
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 position = IN.worldPos.xz;
            int2 cell = round(position);

            half minDist = 999;
            for (int i = 0; i < _Size; i++)
            {
                half dist= sdfBox(position - _Positions[i], half2(.5,.5));
                if(minDist>dist)
                    minDist = dist;
            }

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
            o.Albedo = lerp(grass,grassTinted,grassTint);

            float2 darkGreenUv = position;
            darkGreenUv.x += sin(darkGreenUv.y*2)/16 + sin(darkGreenUv.y*5+.4)/32  + sin(darkGreenUv.y*10+.846)/32;
            fixed3 darkGrass = tex2D (_DarkGreen, TRANSFORM_TEX(darkGreenUv, _DarkGreen) );//tex2D (_Grass, TRANSFORM_TEX(position, _Grass) );
            o.Albedo = lerp(o.Albedo, darkGrass, darkGrassIntensity);

            float2 wheatUv = position;
            wheatUv.xy += sin(wheatUv.x)/32 + sin(wheatUv.x*2.5+.4)/64  + sin(wheatUv.x*10+.846)/32;
            fixed3 wheat = tex2D (_Wheat, TRANSFORM_TEX(wheatUv, _Wheat) );;
            fixed3 wheatTinted = tex2D (_WheatTinted, TRANSFORM_TEX(wheatUv, _Wheat) );;
            fixed3 finalWheat = lerp(wheat,wheatTinted,grassTint);
            o.Albedo = lerp(o.Albedo, finalWheat, wheatIntensity);

            fixed3 yellowGrass = tex2D (_YellowGrass, TRANSFORM_TEX(position, _YellowGrass) );
            o.Albedo = lerp(o.Albedo, yellowGrass, yellowGrassIntensity);

            o.Albedo=lerp(o.Albedo,tex2D (_Ocean, IN.uv_MainTex),1-oceanMask);
            
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;

            //o.Albedo=float3(1,0,0);
            
            o.Emission=border*10*o.Albedo+highlightIntensity*o.Albedo * tex2D (_Grid, position-.5) *7.5;
            o.Emission*= radius;

            float3 normal = UnpackNormal( tex2D (_Normal, TRANSFORM_TEX(position, _Normal) ));
            //normal = sign(normal) * pow(abs(normal),.75);
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
