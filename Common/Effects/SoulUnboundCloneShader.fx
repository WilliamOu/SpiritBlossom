sampler uImage0 : register(s0);
texture bgTexture;
float bgWidth;
float bgHeight;
float bgScrollX;
float bgScrollY;
float colorIntensity;

sampler bg = sampler_state
{
    Texture = (bgTexture);
    AddressU = wrap;
    AddressV = wrap;
    Filter = linear;
};

float4 gradientShader(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 sourceColor = tex2D(uImage0, coords);
    if (dot(sourceColor, sourceColor) == 0) { return sourceColor; }
    
    float grayscale = dot(sourceColor.rgb, float3(0.3, 0.59, 0.11));
    sourceColor.rgb = float3(grayscale, grayscale, grayscale);

    float2 bgCoords = coords;
    bgCoords.x *= bgWidth;
    bgCoords.y *= bgHeight;
    bgCoords.x += bgScrollX;
    bgCoords.y += bgScrollY;

    float4 bgColor = tex2D(bg, bgCoords);
    
    float4 result = sourceColor * bgColor * colorIntensity;
    
    return result;
}

technique Technique1
{
    pass GradientPass
    {
        PixelShader = compile ps_3_0 gradientShader();
    }
}