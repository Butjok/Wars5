struct InstancedRenderingTransform {
    float4x4 mat;
};

#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
    StructuredBuffer<InstancedRenderingTransform> _Transforms;
#endif

void instanced_rendering_vertex(inout InstancedRenderingAppdata v) {
#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

    const float4x4 transform = mul(unity_WorldToObject, _Transforms[v.inst].mat);

    v.vertex = mul(transform, v.vertex);
    //v.tangent = mul(transform, v.tangent);
    v.normal = normalize(mul(transform, v.normal));

#endif 
}