Shader "Custom/InstancedBush"{ //StructuredBuffer+SurfaceShader
 
   // Properties {}
   SubShader {
 
         CGPROGRAM
         
         #pragma target 5.0
         #pragma surface surf Lambert vertex:vert addshadow
         #include "UnityCG.cginc"
 
         struct MeshProperties {
                float4x4 mat;
                float4 color;
            };
 
         #ifdef SHADER_API_D3D11
            StructuredBuffer<MeshProperties> _Properties;
         #endif
 

         struct appdata_t {
             float4 vertex   : POSITION;
             float4 color    : COLOR;
            float3 normal : NORMAL;

            uint id : SV_VertexID;
            uint inst : SV_InstanceID;
            };
 
         struct Input {
            float4 color : COLOR;
         };
 
         void vert (inout appdata_t v) {

            #ifdef SHADER_API_D3D11

            float4x4 transform = mul(unity_WorldToObject, _Properties[v.inst].mat);
            
            v.vertex = mul(transform, v.vertex);
            v.normal = normalize(mul(transform, v.normal));
            
            v.color = _Properties[v.inst].color;
            
            #endif
 
         }
 
 
         void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color;
         }
 
         ENDCG
 
   }
}