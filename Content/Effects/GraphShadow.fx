//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
sampler TextureSampler : register(s0);


float4 PixelShaderBlackFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	return float4(0,0,0,tex2D( TextureSampler, texCoord).a);
	/*if(tex2D( TextureSampler, texCoord).a > 0)
		return float4(0,0,0,1);		
	else
		return float4(0,0,0,0);*/
}


technique Black
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderBlackFunction();
    }
}