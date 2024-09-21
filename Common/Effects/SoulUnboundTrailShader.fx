sampler uImage0 : register(s0);
texture bgTexture;
bool useTransparencyForSecondaryCloudTrail;
float textureLength;
float sourceLength;
float sourceScrollX;
float colorIntensity;

sampler bg = sampler_state
{
    Texture = (bgTexture);
    AddressU = wrap;
    AddressV = wrap;
    // Filter = point; // Adds a mild blur effect when not using point filter
};

float4 gradientShader(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 sourceCoords = coords;
    sourceCoords.x *= sourceLength / textureLength;
    sourceCoords.x += sourceScrollX;

    float4 sourceColor = tex2D(uImage0, sourceCoords);
    float4 bgColor = tex2D(bg, coords);
    
    float4 result = sourceColor * bgColor * colorIntensity;
    
    if (useTransparencyForSecondaryCloudTrail)
    {
        // Adds varied transparency values to the secondary trail to reduce repetitive appearance
        result *= 1.4 * abs(sin(5.2 * (coords.x - 0.5)));
        // result *= -0.6 * cos(9 * (coords.x - 0.5)) + 1 * pow(coords.x - 0.5, 2) + 0.6;
        // result *= -0.7 * cos(9.2 * (coords.x - 0.5)) + 0.7;
    }
    
    return result;
}

technique Technique1
{
    pass GradientPass
    {
        PixelShader = compile ps_3_0 gradientShader();
    }
}