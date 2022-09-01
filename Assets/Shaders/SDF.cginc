half sdfBox(half2 p, half2 size)
{
    half2 d = abs(p) - size;  
    return length(max(d, half2(0,0))) + min(max(d.x, d.y), 0.0);
}

// polynomial one, red
float smin0( float a, float b, float k )
{
    float h = clamp( 0.5 + 0.5*(b-a)/k, 0.0, 1.0 );
    return lerp( b, a, h ) - k*h*(1.0-h);
}

// only works on positive numbers,  green
float smin1(float a, float b, float k)
{
    return pow((0.5 * (pow(a, -k) + pow(b, -k))), (-1.0 / k));
}

// has a log2 off when they are equal,  blue
float smin2(float a, float b, float k)
{
    return -log(exp(-k * a) + exp(-k * b)) / k;
}

// works for both positive and negative numbers and no problem when a == b,  purple
float smin3(float a, float b, float k)
{
    float x = exp(-k * a);
    float y = exp(-k * b);
    return (a * x + b * y) / (x + y);
}

////////////////////////////////////////////////////

float smax0(float a, float b, float k)
{
    return smin1(a, b, -k);
}

float smax1(float a, float b, float k)
{
    return log(exp(k * a) + exp(k * b)) / k;
}

float smax2(float a, float b, float k)
{
    return smin3(a, b, -k);
}