Shader "Custom/Unit"
{
    Properties
    {
        _PlayerColor ("_PlayerColor", Color) = (1,1,1,1)
        _MainTex ("_MainTex", 2D) = "white" {}
        _BounceLight ("_BounceLight", 2D) = "black" {}
        _Occlusion ("_Occlusion", 2D) = "white" {}
        _Roughness ("_Roughness", 2D) = "black" {}
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
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        
        #pragma multi_compile_instancing

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "Assets/Shaders/Utils.cginc"

        sampler2D _MainTex,_Occlusion,_Normal,_Roughness,_Metallic,_BounceLight;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv2_Occlusion;
            float3 worldPos;
        };

        fixed4 _PlayerColor,_Offset,_AttackHighlightColor,_AttackHighlight;
        half _Selected,_HueShift,_BounceIntensity,_OffsetIntensity;
        half _Moved,_AttackHighlightFactor,_AttackHighlightStartTime;
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            half2 uv = IN.uv_MainTex + _Offset.xy*_OffsetIntensity;

            half3 bounce = tex2D (_BounceLight, IN.uv2_Occlusion);

            //bounce=Tint(bounce,_HueShift,1,1);

            
            half3 movedTint = lerp(float3(1,1,1), float3(1,1,1) / 2, _Moved);
            
            // Albedo comes from a texture tinted by color
            fixed3 c = tex2D (_MainTex, uv); // * _PlayerColor;

            //c=Tint(c,_HueShift,1,1);
            
            o.Albedo = c.rgb * movedTint;
            //o.Albedo=tex2D (_Normal, IN.uv_MainTex);
            // Metallic and smoothness come from slider variables
            o.Metallic = tex2D (_Metallic, uv);
            o.Smoothness = 1- tex2D (_Roughness, uv);
            //o.Smoothness = max(o.Smoothness, smoothstep(.1, .0, IN.worldPos.y));
            //if (IN.worldPos.y < .1)
              //  o.Smoothness=1;
            //o.Alpha = c.a;
            o.Occlusion= tex2D (_Occlusion, IN.uv2_Occlusion);
            //o.Albedo = o.Occlusion;
            o.Normal=UnpackNormal(tex2D (_Normal, uv));
            
            o.Emission=c.rgb*bounce.rgb*_BounceIntensity     * movedTint;

            o.Albedo = Tint(o.Albedo,-.0025,1,1) ;//* _PlayerColor;
            
            o.Emission += lerp(_AttackHighlight.x, _AttackHighlight.y, pow
        (sin        ((_Time.y-_AttackHighlightStartTime)*_AttackHighlight.z      )/2+.5, 
            _AttackHighlight.w)) * 
            _AttackHighlightColor *  _AttackHighlightFactor;
            
            /*o.Emission =smoothstep(.8,.9, frac( (IN.worldPos.x + IN
            .worldPos.y 
            + IN
        .worldPos.z)
            *10));*/
            
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
