// A simple post-processing effect that creates a vignette effect.

// The size of the viewport in pixels.
float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

// Scale defines the shape and size of the vignette effect.
float HDRPower = 1.30;

// Power defines the vignette curve.
// 1 .... linear brightness falloff
// >1 ... non-linear brightness falloff
float radius1 = 0.793;

float radius2 = 0.87;

// The texture containing the original image.
texture SourceTexture : SOURCETEXTURE;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = POINT;
};


// Converts a position from screen space to projection space.
// See VSScreenSpaceDraw() in Common.fxh for a detailed description.
void VS(inout float2 texCoord : TEXCOORD0,
        inout float4 position : SV_POSITION)
{
#if !SM4
  position.xy -= 0.5;
#endif
  position.xy /= ViewportSize;
  position.xy *= float2(2, -2);
  position.xy -= float2(1, -1);
}


// Inverts the RGB color.
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float3 color = tex2D(SourceSampler, texCoord).rgb;

	float3 bloom_sum1 = tex2D(SourceSampler, texCoord + float2(1.5, -1.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2(-1.5, -1.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2( 1.5,  1.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2(-1.5,  1.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2( 0.0, -2.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2( 0.0,  2.5) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2(-2.5,  0.0) * radius1).rgb;
	bloom_sum1 += tex2D(SourceSampler, texCoord + float2( 2.5,  0.0) * radius1).rgb;

	bloom_sum1 *= 0.005;

	float3 bloom_sum2 = tex2D(SourceSampler, texCoord + float2(1.5, -1.5) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2(-1.5, -1.5) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2( 1.5,  1.5) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2(-1.5,  1.5) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2( 0.0, -2.5) * radius2).rgb;	
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2( 0.0,  2.5) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2(-2.5,  0.0) * radius2).rgb;
	bloom_sum2 += tex2D(SourceSampler, texCoord + float2( 2.5,  0.0) * radius2).rgb;

	bloom_sum2 *= 0.010;

	float dist = radius2 - radius1;
	float3 HDR = (color + (bloom_sum2 - bloom_sum1)) * dist;
	float3 blend = HDR + color;
	float3 blendAbs = abs(blend);
	float3 powerAbs = abs(HDRPower);
	color = pow(blendAbs, powerAbs) + HDR; // pow - don't use fractions for HDRpower
	
	return float4(saturate(color),1);
}


technique Technique0
{
  pass Pass0
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PS();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}
