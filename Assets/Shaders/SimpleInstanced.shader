Shader "Custom/SimpleInstanced" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_Roughness ("_Roughness", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard addshadow vertex:instanced_rendering_vertex
		#pragma target 3.0
		
		struct InstancedRenderingAppdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;

            uint inst : SV_InstanceID;
        };
        #include "Assets/Shaders/InstancedRendering.cginc"
		
		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		half _Roughness;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex);
			clip(color.a - .5);
			o.Albedo = color.rgb * _Color;
			o.Metallic = 0;
			o.Smoothness = 1 - _Roughness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}