float2 lightSource;
float2 dim;
int lightRadius;
float4 skyColor;
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
    float2 xy = input.TextureCoordinates * dim;
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float blendWeight = saturate(length(xy - lightSource) / lightRadius) * skyColor.a;
    float3 blendedRgb = lerp(color.rgb, skyColor.rgb, blendWeight);
    return float4(blendedRgb, color.a);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_4_0_level_9_1
        MainPS();
    }
};