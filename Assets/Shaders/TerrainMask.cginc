float4 MakeTerrainMask(float factor, float4 thresholds)
{
    if (factor.r < thresholds.x)
        return float4(1,0,0,1);
    if (factor.r < thresholds.y)
        return float4(0,1,0,1);
    if (factor.r < thresholds.z)
        return float4(0,0,1,1);
    return float4(0,0,0,0);
}