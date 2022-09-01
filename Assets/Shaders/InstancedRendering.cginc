struct InstancedRenderingTransform {
    float4x4 mat;
};

#ifdef SHADER_API_D3D11
    StructuredBuffer<InstancedRenderingTransform> _Transforms;
#endif

void instanced_rendering_vertex(inout InstancedRenderingAppdata v) {
#ifdef SHADER_API_D3D11

    const float4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

    v.vertex = mul(transform, v.vertex);
    //v.tangent = mul(transform, v.tangent);
    v.normal = normalize(mul(transform, v.normal));

#endif 
}