//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
texture ScreenTexture;
 
// Our sampler for the texture, which is just going to be pretty simple
sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>;
};
 
//==========================================================================
// Gaussian Blur
//==========================================================================
// Pixel width in texels
float pixelWidth = 1;

#define KERNEL_SIZE 13

float PixelKernel[13] = {-6,-5,
   -4,
   -3,
   -2,
   -1,
    0,
    1,
    2,
    3,
    4,
    5,
    6,
};

static const float BlurWeights[13] =
{
   0.002216,
   0.008764,
   0.026995,
   0.064759,
   0.120985,
   0.176033,
   0.199471,
   0.176033,
   0.120985,
   0.064759,
   0.026995,
   0.008764,
   0.002216,
};

float4 PixelShaderGaussianBlurFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    
    for (int i = 0; i < KERNEL_SIZE; ++i)
        color += tex2D(TextureSampler, TextureCoordinate + PixelKernel[i]) * BlurWeights[i];

   return color;
}


technique GaussianBlur
{
    pass Pass0
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderGaussianBlurFunction();
    }
}


//==========================================================================
// Blur
//==========================================================================
#define BLURRADIUS 3

float4 PixelShaderBlurFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color = float4(0,0,0,0);
	float2 upperCoordinate = TextureCoordinate.xy - 0.01;
	//for(int i = 0; i < 9; i++
	for(int i = 0; i < BLURRADIUS; i++)
	{
		for(int j = 0; j < BLURRADIUS; j++)
		{
			color = color + tex2D(TextureSampler, upperCoordinate + float2(i*0.01,j*0.01));
		}
	}
	
	color.rgb /= BLURRADIUS*BLURRADIUS;


    /*Color =  tex2D(TextureSampler, TextureCoordinate);
    Color += tex2D(TextureSampler, TextureCoordinate.xy + (0.01));
    Color += tex2D(TextureSampler, TextureCoordinate.xy - (0.01));*/
    

    return color;
}


technique Blur
{
    pass Pass0
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderBlurFunction();
    }
}

//==========================================================================
// Invert
//==========================================================================

float4 PixelShaderInvertFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, TextureCoordinate);
	color.rgb = 1 - color.rgb;
    return color;
}
 

technique Invert
{
    pass Pass0
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderInvertFunction();
    }
}

//==========================================================================
// Grayscale
//==========================================================================

float4 PixelShaderGrayScaleFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color;
    color = tex2D( TextureSampler, TextureCoordinate);
	color = dot(color, float3(0.3, 0.59, 0.11));
	color.a = 1;
    return color;
}

technique GrayScale
{
    pass Pass0
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderGrayScaleFunction();
    }
}

//==========================================================================
// Black
//==========================================================================


float4 PixelShaderBlackFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	if(tex2D( TextureSampler, TextureCoordinate).a > 0)
		return float4(0,0,0,1);		
	else
		return float4(0,0,0,0);
		
}

technique Black
{
    pass Pass0
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderBlackFunction();
    }
}