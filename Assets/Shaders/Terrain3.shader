Shader "Custom/Terrain3"
{
    Properties
    {
        _Splat2 ("Splat2", 2D) = "black" {}
		_Normal ("_Normal", 2D) = "bump" {}
        _Grass ("_Grass", 2D) = "white" {}
        _GrassDark ("_GrassDark", 2D) = "white" {}
        _GrassLight ("_GrassLight", 2D) = "white" {}
        
        // Sea
        _SeaColor ("_SeaColor", Color) = (1,1,1,1)
		_DeepSeaColor ("_DeepSeaColor", Color) = (1,1,1,1)
		
		_SeaLevel ("_SeaLevel", Float) = 0
		_SeaThickness ("_SeaThickness", Float) = 0.1
		_SeaSharpness ("_SeaSharpness", Float) = 0.1
		
		_SandColor ("_SandColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "BW"="TrueProbes" }
        Cull off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard 

        struct Input {
            float3 worldPos;
            fixed2 uv_Splat2;
        };

		sampler2D _Splat2, _Normal, _Grass, _GrassDark, _GrassLight;
		fixed2 _Splat2Size;
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
        	
        	fixed3 splat = tex2D(_Splat2, IN.uv_Splat2 / _Splat2Size);
        	fixed darkGrassIntensity = splat.r;
			fixed wheatIntensity = splat.g;
			fixed yellowGrassIntensity = splat.b;
			
			fixed3 grass = tex2D(_Grass, IN.worldPos.xz * 2);
			
			fixed2 grassDarkUv = IN.worldPos.xz;
			grassDarkUv.x += sin(grassDarkUv.y*2)/16 + sin(grassDarkUv.y*5+.4)/32  + sin(grassDarkUv.y*10+.846)/32;
			fixed3 grassDark = tex2D(_GrassDark, grassDarkUv * 3);
			
			fixed3 grassLight = tex2D(_GrassLight, IN.worldPos.xz / 2);
			
			o.Albedo = grass;
			o.Albedo = lerp(o.Albedo, grassDark, darkGrassIntensity);
			o.Albedo = lerp(o.Albedo, grassLight, yellowGrassIntensity);
						
        	o.Normal = UnpackNormal(tex2D(_Normal, IN.worldPos.xz / 5));
        	
        	o.Metallic = 0;
        	o.Smoothness = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
