Shader "Custom/Dissolve" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_NoiseScale ("_NoiseScale", Float) = 0.0
		_Threshold ("_Threshold", Float) = 0.0
		_HeightFactor ("_HeightFactor", Float) = 0.0
		_DissolutionWidth ("_DissolutionWidth", Float) = 0.0
		_DissolutionSharpness ("_DissolutionSharpness", Float) = 0.0
		_Speed ("_Speed", Float) = 0.0
		[HDR] _DissolutionColorYellow ("_DissolutionColorYellow", Color) = (1,1,0,1)
		[HDR] _DissolutionColorRed ("_DissolutionColorRed", Color) = (1,0,0,1)
		_YellowStart ("_YellowStart", Float) = 0.0
		_YellowSharpness ("_YellowSharpness", Float) = 0.0
		_Height ("_Height", Float) = 0.0
		_NoiseScaleBottom ("_NoiseScaleBottom", Float) = 2.0
		_NoiseScaleTop ("_NoiseScaleTop", Float) = 40.0
		
		_MeltWidth ("_MeltWidth", Float) = 0.0
        _MeltBottom ("_MeltBottom", Float) = 0.0
        
        _MeltFactor ("_MeltFactor", Range(0,1)) = 0.0
        _Deformation ("_Deformation", Range(0,1)) = 1.0
        
        _Charcoal ("_Charcoal", Color) = (0,0,0,1)
        _CharcoalWidth ("_CharcoalWidth", Float) = 0.0
        _CharcoalSharpness ("_CharcoalSharpness", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		float wglnoise_mod(float x, float y)
        {
            return x - y * floor(x / y);
        }
        
        float2 wglnoise_mod(float2 x, float2 y)
        {
            return x - y * floor(x / y);
        }
        
        float3 wglnoise_mod(float3 x, float3 y)
        {
            return x - y * floor(x / y);
        }
        
        float4 wglnoise_mod(float4 x, float4 y)
        {
            return x - y * floor(x / y);
        }
        
        float2 wglnoise_fade(float2 t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        
        float3 wglnoise_fade(float3 t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        
        float wglnoise_mod289(float x)
        {
            return x - floor(x / 289) * 289;
        }
        
        float2 wglnoise_mod289(float2 x)
        {
            return x - floor(x / 289) * 289;
        }
        
        float3 wglnoise_mod289(float3 x)
        {
            return x - floor(x / 289) * 289;
        }
        
        float4 wglnoise_mod289(float4 x)
        {
            return x - floor(x / 289) * 289;
        }
        
        float3 wglnoise_permute(float3 x)
        {
            return wglnoise_mod289((x * 34 + 1) * x);
        }
        
        float4 wglnoise_permute(float4 x)
        {
            return wglnoise_mod289((x * 34 + 1) * x);
        }
        
        float ClassicNoise_impl(float3 pi0, float3 pf0, float3 pi1, float3 pf1)
        {
            pi0 = wglnoise_mod289(pi0);
            pi1 = wglnoise_mod289(pi1);
        
            float4 ix = float4(pi0.x, pi1.x, pi0.x, pi1.x);
            float4 iy = float4(pi0.y, pi0.y, pi1.y, pi1.y);
            float4 iz0 = pi0.z;
            float4 iz1 = pi1.z;
        
            float4 ixy = wglnoise_permute(wglnoise_permute(ix) + iy);
            float4 ixy0 = wglnoise_permute(ixy + iz0);
            float4 ixy1 = wglnoise_permute(ixy + iz1);
        
            float4 gx0 = lerp(-1, 1, frac(floor(ixy0 / 7) / 7));
            float4 gy0 = lerp(-1, 1, frac(floor(ixy0 % 7) / 7));
            float4 gz0 = 1 - abs(gx0) - abs(gy0);
        
            bool4 zn0 = gz0 < -0.01;
            gx0 += zn0 * (gx0 < -0.01 ? 1 : -1);
            gy0 += zn0 * (gy0 < -0.01 ? 1 : -1);
        
            float4 gx1 = lerp(-1, 1, frac(floor(ixy1 / 7) / 7));
            float4 gy1 = lerp(-1, 1, frac(floor(ixy1 % 7) / 7));
            float4 gz1 = 1 - abs(gx1) - abs(gy1);
        
            bool4 zn1 = gz1 < -0.01;
            gx1 += zn1 * (gx1 < -0.01 ? 1 : -1);
            gy1 += zn1 * (gy1 < -0.01 ? 1 : -1);
        
            float3 g000 = normalize(float3(gx0.x, gy0.x, gz0.x));
            float3 g100 = normalize(float3(gx0.y, gy0.y, gz0.y));
            float3 g010 = normalize(float3(gx0.z, gy0.z, gz0.z));
            float3 g110 = normalize(float3(gx0.w, gy0.w, gz0.w));
            float3 g001 = normalize(float3(gx1.x, gy1.x, gz1.x));
            float3 g101 = normalize(float3(gx1.y, gy1.y, gz1.y));
            float3 g011 = normalize(float3(gx1.z, gy1.z, gz1.z));
            float3 g111 = normalize(float3(gx1.w, gy1.w, gz1.w));
        
            float n000 = dot(g000, pf0);
            float n100 = dot(g100, float3(pf1.x, pf0.y, pf0.z));
            float n010 = dot(g010, float3(pf0.x, pf1.y, pf0.z));
            float n110 = dot(g110, float3(pf1.x, pf1.y, pf0.z));
            float n001 = dot(g001, float3(pf0.x, pf0.y, pf1.z));
            float n101 = dot(g101, float3(pf1.x, pf0.y, pf1.z));
            float n011 = dot(g011, float3(pf0.x, pf1.y, pf1.z));
            float n111 = dot(g111, pf1);
        
            float3 fade_xyz = wglnoise_fade(pf0);
            float4 n_z = lerp(float4(n000, n100, n010, n110),
                              float4(n001, n101, n011, n111), fade_xyz.z);
            float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
            float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
            return 1.46 * n_xyz;
        }
        
        // Classic Perlin noise
        float ClassicNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            return ClassicNoise_impl(i, f, i + 1, f - 1);
        }
        
        // Classic Perlin noise, periodic variant
        float PeriodicNoise(float3 p, float3 rep)
        {
            float3 i0 = wglnoise_mod(floor(p), rep);
            float3 i1 = wglnoise_mod(i0 + 1, rep);
            float3 f = frac(p);
            return ClassicNoise_impl(i0, f, i1, f - 1);
        }

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 localPos;
		};

		half _Glossiness, _Speed;
		half _Metallic, _NoiseScale, _Threshold, _HeightFactor, _DissolutionWidth, _DissolutionSharpness;
		half _YellowStart, _YellowSharpness;
		half _NoiseScaleBottom, _NoiseScaleTop, _Height;
		fixed4 _Color, _DissolutionColorYellow, _DissolutionColorRed, _Charcoal;
		half _MeltWidth, _MeltBottom, _MeltFactor, _Deformation, _CharcoalWidth, _CharcoalSharpness;
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex;
//			v.vertex.y += _Speed * _Threshold;

			half height = v.vertex.y;
			half meltTop = _MeltBottom + _MeltWidth;
			
			/*half noise = (ClassicNoise(v.vertex * float3(1, 0, 1) * _NoiseScale) + 1) / 2 *.33 + .66;
			v.vertex.y = lerp(v.vertex.y, v.vertex.y * noise, smoothstep(0, 1, saturate(_MeltFactor)));
			v.vertex.x = lerp(v.vertex.x, v.vertex.x / sqrt(noise), smoothstep(0, 1, saturate(_MeltFactor)));
			v.vertex.z = lerp(v.vertex.z, v.vertex.z / sqrt(noise), smoothstep(0, 1, saturate(_MeltFactor)));
			
			v.vertex.y -= _Speed * _Threshold;*/
			
			if (_Deformation > .5){
			
			half a = saturate(1 - saturate((_Threshold - -1) / (.5 - -1)));
			a = 1 - (1 - a) * (1 - a);
			//a = 1 - pow(2, -10 * a);
			//a = sin(3.1415/2 * a);
			half b = a * 1 + 1;
			v.vertex.xz *= b;
			v.vertex.y -= .5;
			v.vertex.y /= b*b;
			v.vertex.y += .5;
			//v.vertex.y += a*3;
			
			}
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			
			//float alpha = ClassicNoise((IN.localPos * _HeightFactor + _Threshold) * _NoiseScale);
			
			float noiseScale = lerp(_NoiseScaleBottom, _NoiseScaleTop, sqrt(sqrt(saturate(IN.localPos.y / _Height))));
			float3 samplePoint = IN.localPos;
			samplePoint.x *= noiseScale;
			samplePoint.z *= noiseScale;
			samplePoint.y *= noiseScale;
			
			half a = saturate(1 - saturate((_Threshold - -1) / (.5 - -1)));
			a = 1 - (1 - a) * (1 - a);
			half b = a * 1 + 1;
			
			samplePoint.y /= b*b;
			
			float noise = 0;
			noise += (ClassicNoise(samplePoint));
			noise += ClassicNoise(samplePoint * 2) / 2;
			noise += ClassicNoise(samplePoint * 4) / 4;
			noise += ClassicNoise(samplePoint * 8) / 8;
			//noise += ClassicNoise(samplePoint * 16) / 16;
			//noise += ClassicNoise(samplePoint * 32) / 32;
			//noise += ClassicNoise(samplePoint * 64) / 64;
			//noise += ClassicNoise(IN.localPos * _NoiseScale * 4) / 4;
			
			half alpha = noise - IN.localPos.y * _HeightFactor + _Threshold;
			clip(alpha);
			
			half emissionFactor = smoothstep(_DissolutionWidth+_DissolutionSharpness, _DissolutionWidth, alpha);
			o.Emission = lerp(_DissolutionColorRed, _DissolutionColorYellow, smoothstep(_YellowStart, _YellowStart + _YellowSharpness, alpha)) * emissionFactor;
			
			half charcoalFactor = smoothstep(_CharcoalWidth+_CharcoalSharpness, _CharcoalWidth, alpha);
			o.Albedo = lerp(o.Albedo, _Charcoal, charcoalFactor);
		}
		ENDCG
	}
	FallBack "Diffuse"
}