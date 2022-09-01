half3 hue_shift(half3 color, const in float shift)
{
    const half3 p = half3(0.55735, 0.55735, 0.55735) * dot(half3(0.55735, 0.55735, 0.55735), color);
    const half3 u = color - p;
    const half3 v = cross(half3(0.55735, 0.55735, 0.55735), u);
    color = u * cos(shift * 6.2832) + v * sin(shift * 6.2832) + p;
    return color;
}
