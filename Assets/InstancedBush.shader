Shader "Custom/InstancedBush"{ //StructuredBuffer+SurfaceShader
 
   // Properties {}
   SubShader {
 
      CGPROGRAM
      
      #pragma target 5.0
      #pragma surface surf Lambert vertex:vert addshadow
      #include "UnityCG.cginc"

      struct Transform {
         float4x4 mat;
      };
      
      #ifdef SHADER_API_D3D11
         StructuredBuffer<Transform> _Transforms;
      #endif

      struct appdata_t {
          float4 vertex : POSITION;
          float4 tangent : TANGENT;
          float3 normal : NORMAL;
          float4 texcoord : TEXCOORD0;
          float4 texcoord1 : TEXCOORD1;
          float4 texcoord2 : TEXCOORD2;
          float4 texcoord3 : TEXCOORD3;
          fixed4 color : COLOR;

          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
      };

      struct Input {
         float4 color : COLOR;
      };

      void vert (inout appdata_t v) {
         #ifdef SHADER_API_D3D11

         float4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);
         
         v.vertex = mul(transform, v.vertex);
         v.normal = normalize(mul(transform, v.normal));
         
         #endif 
      }

      void surf (Input IN, inout SurfaceOutput o) {
         o.Albedo = IN.color;
      }

      ENDCG 
   }
}