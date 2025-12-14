#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

float Saturation;
float Contrast;
float3 Tint;

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
    float4 fullColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float alpha = fullColor.a;
    float3 color = fullColor.rgb;
    
    // Saturation
    float gray = dot(color, float3(0.299, 0.587, 0.114));
    color = lerp(float3(gray, gray, gray), color, Saturation);

    // Contrast
    color = (color - 0.5) * Contrast + 0.5;
    
    // Tint
    color *= Tint;
    
    // Clamp
    color = saturate(color);
    
    return float4(color, alpha);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL
        MainPS();
    }
};