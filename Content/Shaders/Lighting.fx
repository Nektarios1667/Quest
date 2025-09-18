#define MAX_LIGHTS 12

float3 lightSources[MAX_LIGHTS];
float2 dim;
float4 skyColor;
float4 lightColors[MAX_LIGHTS];
Texture2D SpriteTexture;
int numLights;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 xy = input.TextureCoordinates * dim;
    float4 baseColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    float3 totalLight = baseColor.rgb;

    float brightness = 0;

    for (int i = 0; i < numLights; ++i)
    {
        float2 delta = xy - lightSources[i].xy;
        float distSq = dot(delta, delta);
        float radiusSq = lightSources[i].z * lightSources[i].z;

        float weight = saturate(1.0 - (distSq / radiusSq));
        weight *= weight;

        // Reduce skytint
        brightness += lightColors[i].a * weight;

        // Non-premultiplied tint
        totalLight += lightColors[i].rgb * weight;
    }

    // Clamp
    brightness = saturate(brightness);

    // Lerp with sky color based on brightness
    float3 blendedRgb = lerp(totalLight, skyColor.rgb, (1.0 - brightness) * skyColor.a);
    
    return float4(saturate(blendedRgb), baseColor.a);
}



technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_4_0_level_9_3 MainPS();
    }
};