Shader "Custom/Unit"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _BounceLight ("_BounceLight", 2D) = "black" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "white" {}
        _Normal ("_Normal", 2D) = "bump" {}
        _Metallic ("_Metallic", 2D) = "black" {}
        _HueShift ("_HueShift", Range(0,1)) = 1.0
        _BounceIntensity ("_BounceIntensity", Range(0,10)) = 1.0
        _Offset ("_Offset", Vector) = (0, 0, .5, 1)
        _OffsetIntensity ("_OffsetIntensity", Range(0,10)) = 0
        _Moved ("_Moved", Range(0,1)) = 0
        _AttackHighlightFactor ("_AttackHighlightFactor", Range(0,1)) = 0
        _AttackHighlightColor ("_AttackHighlightColor", Color) = (1,1,1,1)
        _AttackHighlight ("_AttackHighlight", Vector) = (.25, .5, 5.0, 2.5)
        _RedAmount ("_RedAmount", Range(0,1)) = 0
        _DamageFalloffIntensity ("_DamageFalloffIntensity", Float) = 10
        _DamageTime ("_DamageTime", Float) = -1000
        [Toggle(HOLE)] _Hole ("_Hole", Float) = 0
        _HoleRadius ("_HoleRadius", Float) = 0.5
        _Noise ("_Noise", 2D) = "black" {}
        [Toggle(DISSOLVE)] _DissolveToggle ("_DissolveToggle", Float) = 0
        _ClipThreshold ("_ClipThreshold", Float) = 0.5
        _NoiseScale ("_NoiseScale", Float) = 1
        [HDR] _FireColor ("_FireColor", Color) = (1,1,1,1)
        _FireIntensity ("_FireIntensity", Range(0,1)) = 1
        _FireThickness ("_FireThickness", Range(0,1)) = 1
        _FireSmoothness ("_FireSmoothness", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        Stencil
        {
            Ref 1
            Comp always
            Pass replace
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow
        #pragma shader_feature HOLE
        #pragma shader_feature DISSOLVE

        #pragma multi_compile_instancing

        #pragma target 3.5

        #include "Assets/Shaders/Utils.cginc"

        sampler2D _MainTex, _Occlusion, _Normal, _Roughness, _Metallic, _BounceLight;

        struct Input {
            float2 uv_MainTex;
            float2 uv2_Occlusion;

            #if HOLE
            float3 worldPos;
            float3 objectWorldPosition;
            float4 hole;
            #endif

            #if DISSOLVE
            float3 projectionAlphas;
            float3 objectPosition;
            #endif
        };

        fixed4 _Offset, _AttackHighlightColor, _AttackHighlight;
        half _Selected, _HueShift, _BounceIntensity;
        half _AttackHighlightFactor, _AttackHighlightStartTime;
        half _DamageFalloffIntensity;

        #if DISSOLVE
        sampler2D _Noise;
        float _ClipThreshold;
        float _NoiseScale;
        float3 _FireColor;
        float _FireIntensity;
        float _FireThickness;
        float _FireSmoothness;
        #endif

        #if HOLE
        half _HoleRadius;
        sampler2D _HoleMask;
        fixed4x4 _HoleMask_WorldToLocal;
        #endif

        float3 ToRed(float3 blue) {
            float3 hsv = RGBtoHSV(blue);
            float hue = hsv.x;

            float newHue = 0.0;
            if (hue < 0.622) {
                newHue = lerp(1, 0.015626, hue / 0.622);
            }
            else if (hue < 0.653) {
                newHue = lerp(0.015626, 0, (hue - 0.622) / (0.653 - 0.622));
            }
            else if (hue < 0.710) {
                newHue = lerp(0, 0.011, (hue - 0.653) / (0.710 - 0.653));
            }
            else if (hue < 1.00) {
                newHue = lerp(0.011, 0, (hue - 0.710) / (1.0 - 0.710));
            }

            return HSVtoRGB(float3(newHue, hsv.y, hsv.z));
        }

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

			#if HOLE
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.objectWorldPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            o.hole = tex2Dlod(_HoleMask, float4(mul(_HoleMask_WorldToLocal, float4(o.objectWorldPosition, 1)).xz, 0, 0));
			#endif

            #if DISSOLVE
            o.objectPosition = v.vertex.xyz * _NoiseScale;
            float xAlpha = abs(dot(v.normal, float3(1, 0, 0)));
            float yAlpha = abs(dot(v.normal, float3(0, 1, 0)));
            float zAlpha = abs(dot(v.normal, float3(0, 0, 1)));
            float sum = xAlpha + yAlpha + zAlpha;
            xAlpha /= sum;
            yAlpha /= sum;
            zAlpha /= sum;
            o.projectionAlphas = float3(xAlpha, yAlpha, zAlpha);
            #endif
        }

        float InverseLerp(float from, float to, float value) {
            return (value - from) / (to - from);
        }

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float, _OffsetIntensity)
        UNITY_DEFINE_INSTANCED_PROP(float, _RedAmount)
        UNITY_DEFINE_INSTANCED_PROP(float, _Moved)
        UNITY_DEFINE_INSTANCED_PROP(float, _DamageTime)
        UNITY_DEFINE_INSTANCED_PROP(float, _DissolveTime)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o) {
            
        	#if HOLE
            if (IN.hole.a > 0.5) {
                float3 direction = normalize(IN.worldPos - _WorldSpaceCameraPos);
                float3 projectedPoint = RayPlaneIntersection(_WorldSpaceCameraPos, direction, IN.objectWorldPosition, direction);

                float distance = length(projectedPoint - IN.objectWorldPosition) - _HoleRadius;
                clip(distance);
            }
        	#endif

            half2 uv = IN.uv_MainTex + _Offset.xy * UNITY_ACCESS_INSTANCED_PROP(Props, _OffsetIntensity);

            half3 bounce = tex2D(_BounceLight, IN.uv2_Occlusion) + .001;
            float redAmount = UNITY_ACCESS_INSTANCED_PROP(Props, _RedAmount);
            bounce = lerp(bounce, ToRed(bounce), redAmount);

            float moved = UNITY_ACCESS_INSTANCED_PROP(Props, _Moved);

            half3 movedTint = lerp(float3(1, 1, 1), float3(1, 1, 1) / 10, moved);

            fixed3 c = tex2D(_MainTex, uv);
            c = lerp(c, ToRed(c), redAmount);

            o.Albedo = c.rgb * movedTint;
            o.Metallic = tex2D(_Metallic, uv);
            o.Smoothness = (1 - tex2D(_Roughness, uv)) * lerp(1, .66, moved);
            o.Occlusion = tex2D(_Occlusion, IN.uv2_Occlusion);
            o.Normal = UnpackNormal(tex2D(_Normal, uv));

            bounce *= (1 - o.Metallic);
            bounce *= (1 - moved);

            o.Emission = bounce.rgb * _BounceIntensity * movedTint;

            float3 _DamageColor = float3(.25, .25, 0);
            o.Emission += _DamageColor * saturate(exp(-(_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _DamageTime)) * 40));

            #if DISSOLVE

            float timeElapsed = _Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _DissolveTime);
            float fireIntensity = smoothstep(0, .25, timeElapsed);
            float clipOffset = lerp(-.001, 1, (saturate(InverseLerp(0, 2, timeElapsed))));

            float xSample = tex2D(_Noise, IN.objectPosition.yz).a;
            float ySample = tex2D(_Noise, IN.objectPosition.xz).a;
            float zSample = tex2D(_Noise, IN.objectPosition.xy).a;
            float averageSample = 1 - (xSample + ySample + zSample) / 3;
            clip(averageSample - clipOffset);

            o.Emission += smoothstep(clipOffset + _FireThickness + _FireSmoothness, clipOffset + _FireThickness, averageSample) * _FireColor * fireIntensity;

            #endif
        }
        ENDCG
    }
}