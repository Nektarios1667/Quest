#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

int PixelSize;
float2 TexSize;

Texture2D SpriteTexture;

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
    float2 uv = input.TextureCoordinates;
    float2 pixelCoord = uv * TexSize;
    float2 pixelatedCoord = floor(pixelCoord / PixelSize) * PixelSize;
    uv = pixelatedCoord / TexSize;
    
    return tex2D(SpriteTextureSampler, uv) * input.Color;

}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL
        MainPS();
    }
};