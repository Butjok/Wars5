Shader "Instanced/InstancedSurfaceShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _CutOff ("CutOff", Range(0,1)) = 0.5
        _ShadowCutOff ("_ShadowCutOff", Range(0,1)) = 0.5
        _UpNormal ("_UpNormal", Vector) = (0,0,1)
        _NormalWrap ("Normal Wrap", Range(-1,1)) = 0.5
        _Splat ("Splat", 2D) = "white" {}

        _Grass ("_Grass", Color) = (1,1,1,1)
        _DarkGrass ("_DarkGrass", Color) = (1,1,1,1)
        _YellowGrass ("_YellowGrass", Color) = (1,1,1,1)
        _Forest ("_Forest", Color) = (1,1,1,1)
        _GrassTint ("_TintMask", 2D) = "black" {}

        _VertexOffset ("Vertex Offset", Vector) = (0,0,0)
        
        [Toggle(WIND)] _Wind ("Wind", Float) = 0
        
        [Toggle(HOLE)] _Hole ("_Hole", Float) = 0
    	_EmissionColor ("_EmissionColor", Color) = (1,1,1,1)
    	_HoleOffset ("_HoleOffset", Vector) = (0,0,0,0)
    	
    	_HoleRadius ("_HoleRadius", Float) = 0.1
		_TerrainHeight ("_TerrainHeight", 2D) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "LightMode"="Deferred" 
        }
        LOD 200

        CGPROGRAM

        #pragma target 5.0
        
        #pragma surface surf Standard vertex:vert addshadow
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma shader_feature WIND
        #pragma shader_feature HOLE

        #include "Utils.cginc"

        struct Input {
            float2 uv_MainTex;
            float3 color;
            float3 worldPos;
            #if HOLE
                float4 objectWorldPos;
            #endif
        };

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4x4> _Transforms;
        StructuredBuffer<float4x4> _InverseTransforms;
    #endif

        int _IndexOffset;

        float _CutOff;
        float _ShadowCutOff;

        float3 _UpNormal;
        float _NormalWrap;

        sampler2D _MainTex;

        sampler2D _Splat;
        float4x4 _Splat_WorldToLocal;

        sampler2D _ForestMask;
        float4x4 _ForestMask_WorldToLocal;

        sampler2D _GrassTint;

        float3 _Grass;
        float3 _DarkGrass;
        float3 _YellowGrass;
        float3 _Forest;
        float3 _VertexOffset;
        
        #if HOLE

        sampler2D _TerrainHeight;
        float4x4 _WorldToTerrainHeightUv;
        sampler2D _HoleMask;
        float4x4 _HoleMask_WorldToLocal;
        
        float3 _HoleOffset;
        float _HoleRadius;
        float3 _EmissionColor;

        #endif


        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            #if WIND
				half intensity = length(v.vertex.xz);
				half wind = sin(_Time.y*7 - v.vertex.x*4)/2+.5;
				v.vertex += wind * intensity * float4(1,0,0,0) * sin(_Time.y*4 + v.vertex.z*15)*.04;
				v.vertex += wind * intensity * float4(0,0,1,0) * sin(_Time.y*5 + v.vertex.x*12.4535)*.04;
            #endif

            v.vertex.xyz += _VertexOffset;
            v.normal = lerp(v.normal, _UpNormal, _NormalWrap);

            float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
            float4 splat = tex2Dlod(_Splat, float4(mul(_Splat_WorldToLocal, float4(worldPos, 1)).xz, 0, 0));
            o.color = _Grass;
            o.color = lerp(o.color, _DarkGrass, splat.r);
            o.color = lerp(o.color, _YellowGrass, splat.b);

            float forest = tex2Dlod(_ForestMask, float4(mul(_ForestMask_WorldToLocal, float4(worldPos, 1)).xz, 0, 0)).r;
            o.color = lerp(o.color, _Forest, forest);

            #if HOLE
        		float3 objectWorldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float4 tilePos = float4(round(objectWorldPos.x), 0, round(objectWorldPos.z), 1);
                o.objectWorldPos = float4(
            		tilePos.x,
					tex2Dlod(_TerrainHeight, float4(mul(_WorldToTerrainHeightUv, tilePos).xz, 0, 0)).r,
            		tilePos.z,
            		tex2Dlod(_HoleMask, float4(mul(_HoleMask_WorldToLocal, tilePos).xz, 0, 0)).r);
            	
				o.objectWorldPos.xyz += _HoleOffset;
            #endif
        }

        void setup() {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            unity_ObjectToWorld = _Transforms[_IndexOffset + unity_InstanceID];
            unity_WorldToObject = _InverseTransforms[_IndexOffset + unity_InstanceID];
        #endif
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            float cutOff;
            #ifdef UNITY_PASS_SHADOWCASTER
                cutOff = _ShadowCutOff;
	        #else
            cutOff = _CutOff;
	        #endif
            // https://bgolus.medium.com/anti-aliased-alpha-test-the-esoteric-alpha-to-coverage-8b177335ae4f ???
            c.a = (c.a - cutOff) / max(fwidth(c.a), 0.0001) + 0.5;
            clip(c.a);

            o.Metallic = 0;
            o.Smoothness = 0;
            o.Albedo = IN.color;

            float grassTint = tex2D(_GrassTint, IN.worldPos.xz * .5).r;
            float3 albedoHSV2 = RGBtoHSV(o.Albedo);
            albedoHSV2.y *= 1.11;
            albedoHSV2.z *= .65;
            o.Albedo = lerp(o.Albedo, HSVtoRGB(albedoHSV2), grassTint);

			#if HOLE
				if (IN.objectWorldPos.a > 0.1) {
					float3 holePosition = IN.objectWorldPos.xyz;
				
					float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
					float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, holePosition, direction);
							
					float distance = length(projectedPoint - holePosition) - _HoleRadius;
					clip(distance);

					float circleBorder = smoothstep(0.025, 0.015, distance);
					o.Emission = circleBorder * _EmissionColor;
					o.Albedo = lerp(o.Albedo, 0, circleBorder);
				}
        	#endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}