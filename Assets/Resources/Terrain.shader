Shader "Custom/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Grass ("_Grass", 2D) = "white" {}
        _Grid ("_Grid", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Size ("_Size", Range(0,64)) = 0.0
        _Radius ("_Radius", Range(0,10)) = 0.0
        _K ("_K", Range(0,10)) = 0.0
        _Rounding ("_Rounding", Range(-1,1)) = 0.0
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

        sampler2D _Grass,_Grid;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic,_Radius,_Rounding,_K,_SelectTime,_TimeDirection;
        fixed4 _Color;

        #define SIZE 64
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

            half radiusDistance = length(_From-position)-(_Time.y - _SelectTime)*45*_TimeDirection;
            //minDist = max(minDist,radiusDistance);
            
            half highlightIntensity = smoothstep(.01,0.005,minDist-_Rounding);
            half border = smoothstep(.0075 + .0025,0.0075,abs(0.0-(minDist-_Rounding)));
            half3 highlight = half3(1,1,1)/3;
            
            half radius = smoothstep(_Radius+.1,_Radius, radiusDistance);
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_Grass, position);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = c.a;

            o.Emission=border*7.50*c.rgb+highlightIntensity*c.rgb * tex2D (_Grid, position-.5) *10;
            o.Emission*= radius;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
