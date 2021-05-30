//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com/
// Copyright (C) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file Material.fx
/// Combines the material of a model (e.g. textures) with the light buffer data.
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
//
//-----------------------------------------------------------------------------

#include "References/Common.fxh"
#include "References/Encoding.fxh"
#include "References/Deferred.fxh"
#include "References/Material.fxh"
#include "References/Noise.fxh"

// Sceelix Custom Shader-------------------------------------------------------

float2 Texture1Tiling;
texture Texture1; \
sampler Texture1Sampler = sampler_state \
{ \
  Texture = <Texture1>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture2Tiling;
texture Texture2; \
sampler Texture2Sampler = sampler_state \
{ \
  Texture = <Texture2>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture3Tiling;
texture Texture3; \
sampler Texture3Sampler = sampler_state \
{ \
  Texture = <Texture3>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture4Tiling;
texture Texture4; \
sampler Texture4Sampler = sampler_state \
{ \
  Texture = <Texture4>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture5Tiling;
texture Texture5; \
sampler Texture5Sampler = sampler_state \
{ \
  Texture = <Texture5>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture6Tiling;
texture Texture6; \
sampler Texture6Sampler = sampler_state \
{ \
  Texture = <Texture6>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture7Tiling;
texture Texture7; \
sampler Texture7Sampler = sampler_state \
{ \
  Texture = <Texture7>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

float2 Texture8Tiling;
texture Texture8; \
sampler Texture8Sampler = sampler_state \
{ \
  Texture = <Texture8>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines
//#define ALPHA_TEST 1
//#define TRANSPARENT 1
//#define EMISSIVE 1
//#define MORPHING 1
//#define SKINNING 1

#if ALPHA_TEST
#define CULL_MODE NONE
#else
#define CULL_MODE CCW
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
#if EMISSIVE
float3 EmissiveColor : EMISSIVECOLOR;
#endif
#if ALPHA_TEST
float ReferenceAlpha : REFERENCEALPHA = 0.9f;
#endif
#if TRANSPARENT
float InstanceAlpha : INSTANCEALPHA = 1;
#endif
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)

#if MORPHING
float MorphWeight0 : MORPHWEIGHT0;
float MorphWeight1 : MORPHWEIGHT1;
float MorphWeight2 : MORPHWEIGHT2;
float MorphWeight3 : MORPHWEIGHT3;
float MorphWeight4 : MORPHWEIGHT4;
#endif

#if SKINNING
float4x3 Bones[72];
#endif


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : SV_POSITION;
  float2 TexCoord : TEXCOORD0;
  float4 Color : COLOR0;
  float4 Color1 : COLOR1;
#if MORPHING
  float3 MorphPosition0 : POSITION1;
  float3 MorphPosition1 : POSITION2;
  float3 MorphPosition2: POSITION3;
  float3 MorphPosition3 : POSITION4;
  float3 MorphPosition4 : POSITION5;
#endif
#if SKINNING
  int4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float4 Color : COLOR0;
  float4 Color1 : COLOR1;
#if TRANSPARENT
  float4 InstanceColorAndAlpha : TEXCOORD2;
#endif
  float4 Position : SV_POSITION;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float4 Color : COLOR0;
  float4 Color1 : COLOR1;
#if TRANSPARENT
  float4 InstanceColorAndAlpha : TEXCOORD2;
#if SM4
  float4 VPos : SV_POSITION;
#else
  float2 VPos : VPOS;
#endif
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float4 instanceColorAndAlpha)
{
  float4 position = input.Position;
  
#if MORPHING
  // ----- Apply morph targets.
  position.xyz += MorphWeight0 * input.MorphPosition0;
  position.xyz += MorphWeight1 * input.MorphPosition1;
  position.xyz += MorphWeight2 * input.MorphPosition2;
  position.xyz += MorphWeight3 * input.MorphPosition3;
  position.xyz += MorphWeight4 * input.MorphPosition4;
#endif

#if SKINNING
  // ----- Apply skinning matrix.
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  position.xyz = mul(position, skinningMatrix);
#endif
  
  // ----- Apply world, view, projection transformation.  
  float4 positionWorld = mul(position, world);
  float4 positionView = mul(positionWorld, View);
  float4 positionProj = mul(positionView, Projection);

  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  output.TexCoord = input.TexCoord;
  output.PositionProj = positionProj;
  output.Color = input.Color;//float4(input.Color.x/255.0,input.Color.y/255.0,input.Color.z/255.0,input.Color.w/255.0);
  output.Color1 = input.Color1;
#if TRANSPARENT
  output.InstanceColorAndAlpha = instanceColorAndAlpha;
#endif
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
#if TRANSPARENT
  return VS(input, World, float4(0, 0, 0, InstanceAlpha));
#else
  return VS(input, World, 0);
#endif
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(input, world, colorAndAlpha);
}


float4 PS(PSInput input) : COLOR0
{
    float4 diffuseMap = tex2D(Texture1Sampler, input.TexCoord * Texture1Tiling)*input.Color.x;
		diffuseMap += tex2D(Texture2Sampler, input.TexCoord * Texture2Tiling)*input.Color.y;
		diffuseMap += tex2D(Texture3Sampler, input.TexCoord * Texture3Tiling)*input.Color.z;
		diffuseMap += tex2D(Texture4Sampler, input.TexCoord * Texture4Tiling)*input.Color.w;
		
		diffuseMap += tex2D(Texture5Sampler, input.TexCoord * Texture5Tiling)*input.Color1.x;
		diffuseMap += tex2D(Texture6Sampler, input.TexCoord * Texture6Tiling)*input.Color1.y;
		diffuseMap += tex2D(Texture7Sampler, input.TexCoord * Texture7Tiling)*input.Color1.z;
		diffuseMap += tex2D(Texture8Sampler, input.TexCoord * Texture8Tiling)*input.Color1.w;
		
  
#if ALPHA_TEST
  clip(diffuseMap.a - ReferenceAlpha);
#endif
#if TRANSPARENT
  // Screen-door transparency
  float c = input.InstanceColorAndAlpha.a - Dither4x4(input.VPos.xy);
  // The alpha can be negative, which means the dither pattern is inverted.
  if (input.InstanceColorAndAlpha.a < 0)
    c = -(c + 1);
  
  clip(c);
#endif
  
  float4 specularMap = tex2D(SpecularSampler, input.TexCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  

  
#if EMISSIVE
  float emissive = specularMap.a;
#endif
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
#if EMISSIVE
  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight + EmissiveColor * diffuse * emissive, 1);
#else
  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 1);
#endif
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SKINNING && !MORPHING && !MGFX           // TODO: Add Annotation support to MonoGame.
#define SUPPORTS_INSTANCING 1
#endif

technique Default
#if SUPPORTS_INSTANCING
< string InstancingTechnique = "Instancing"; >
#endif
{
  pass
  {
    CullMode = CULL_MODE;
    
#if !SM4 && !TRANSPARENT
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#elif !SM4
    VertexShader = compile vs_3_0 VSNoInstancing();
    PixelShader = compile ps_3_0 PS();                // ps_3_0 required for VPOS.
#elif SM4 && !TRANSPARENT
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#elif SM4
    VertexShader = compile vs_4_0 VSNoInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

#if SUPPORTS_INSTANCING
technique Instancing
{
  pass
  {
    CullMode = CULL_MODE;
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
  }
}
#endif
