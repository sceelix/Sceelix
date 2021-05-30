//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com/
// Copyright (C) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file Deferred.fxh
/// Functions for deferred rendering (e.g. G-buffer and light buffer access).
/// 
/// If you are creating the G-buffer, you need to define
///   #define CREATE_GBUFFER 1
/// before including Deferred.fxh.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_DEFERRED_FXH
#define DIGITALRUNE_DEFERRED_FXH

#ifndef DIGITALRUNE_ENCODING_FXH
#error "Encoding.fxh required. Please include Encoding.fxh before including Deferred.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

#if CREATE_GBUFFER
DECLARE_UNIFORM_NORMALSFITTINGTEXTURE
#endif


/// Declares the uniform const for the view frustum far corners in view space.
/// \param[in] name   The name of the constant.
/// \remarks
/// Order of the corners: (top left, top right, bottom left, bottom right)
/// Usually you will call
///   DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
#define DECLARE_UNIFORM_FRUSTUMCORNERS(name) float3 name[4]


/// Declares the uniform const for a G-buffer texture + sampler.
/// \param[in] name   The name of the texture constant.
/// \param[in] index  The index of the G-buffer.
/// \remarks
/// Example: To declare GBuffer0 and GBuffer0Sampler call
///   DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
///  DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
#define DECLARE_UNIFORM_GBUFFER(name, index) \
texture name : GBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MipFilter = POINT; \
  MinFilter = POINT; \
  MagFilter = POINT; \
}


/// Declares the uniform const for a light buffer texture + sampler.
/// \param[in] name   The name of the light buffer constant.
/// \param[in] index  The index of the light buffer.
/// \remarks
/// Example: To declare LightBuffer0 and LightBuffer0Sampler call
///   DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);
#define DECLARE_UNIFORM_LIGHTBUFFER(name, index) \
texture name : LIGHTBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MipFilter = POINT; \
  MinFilter = POINT; \
  MagFilter = POINT; \
}


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Gets the linear depth in the range [0,1] from a G-buffer 0 sample.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \return The linear depth in the range [0, 1].
float GetGBufferDepth(float4 gBuffer0)
{
  return abs(gBuffer0.x);
}


#if CREATE_GBUFFER
/// Stores the depth in the given G-buffer 0 value.
/// \param[in] depth          The linear depth in the range [0, 1].
/// \param[in] sceneNodeType  The scene node type info (1 = static, 0 = dynamic).
/// \param[in,out] gBuffer0   The G-buffer 0 value.
void SetGBufferDepth(float depth, float sceneNodeType, inout float4 gBuffer0)
{
  if (sceneNodeType)
  {
    // Static objects are encoded as positive values.
    gBuffer0 = depth;
  }
  else
  {
    // Dynamic objects are encoded as negative values.
    gBuffer0 = -depth;
  }
}
#endif


/// Gets the world space normal from a G-buffer 1 sample.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The normal in world space.
float3 GetGBufferNormal(float4 gBuffer1)
{
  return DecodeNormalBestFit((half4)gBuffer1);
}

#if CREATE_GBUFFER
/// Stores the world space normal in the given G-buffer 1 value.
/// \param[in] normal         The normal in world space.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferNormal(float3 normal, inout float4 gBuffer1)
{
  gBuffer1.rgb = EncodeNormalBestFit((half3)normal, NormalsFittingSampler);
}
#endif


/// Gets the specular power from the given G-buffer samples.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The specular power.
float GetGBufferSpecularPower(float4 gBuffer0, float4 gBuffer1)
{
  return DecodeSpecularPower(gBuffer1.a);
}

/// Stores the given specular power in the G-buffer.
/// \param[in] specularPower  The specular power.
/// \param[in,out] gBuffer0   The G-buffer 0 value.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferSpecularPower(float specularPower, inout float4 gBuffer0, inout float4 gBuffer1)
{
  gBuffer1.a = EncodeSpecularPower(specularPower);
}

/// Gets the diffuse light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The diffuse light value.
float3 GetLightBufferDiffuse(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer0.xyz;
}


/// Gets the specular light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The specular light value.
float3 GetLightBufferSpecular(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer1.xyz;
}


/// Gets the index of the given texture corner.
/// \param[in] texCoord The texture coordinate of one of the texture corners.
///                     Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
/// \return The index of the texture corner.
/// \retval 0   left, top
/// \retval 1   right, top
/// \retval 2   left, bottom
/// \retval 3   right, bottom
float GetCornerIndex(in float2 texCoord)
{
  return texCoord.x + (texCoord.y * 2);
}


/// A vertex shader like Common.fxh/VSScreenSpaceDraw, which also
/// outputs the frustum ray for this vertex.
/// \param[in,out] texCoord       In: The texture coordinate of one of the texture corners.
///                               Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
///                               Out: The texture coordinates of the vertex.
/// \param[in]     viewportSize   The viewport size in pixels.
/// \param[in]     frustumCorners See constant FrustumCorners above.
void VSFrustumRay(inout float2 texCoord : TEXCOORD0,
                  out float3 frustumRay : TEXCOORD1,
                  inout float4 position : SV_POSITION,
                  uniform const float2 viewportSize,
                  uniform const float3 frustumCorners[4])
{
  frustumRay = frustumCorners[GetCornerIndex(texCoord)];
  
  position.xy /= viewportSize;
  
  texCoord.xy = position.xy;
  
  // Instead of subtracting the 0.5 pixel offset from the position, we add
  // it to the texture coordinates - because frustumRay is associated with
  // the position output.
#if !SM4
  texCoord.xy += 0.5f / viewportSize;
#endif
  
  position.xy = position.xy * float2(2, -2) - float2(1, -1);
}
#endif
