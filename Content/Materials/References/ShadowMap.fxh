//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com/
// Copyright (C) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file ShadowMap.fxh
/// Useful functions for shadow mapping.
//
// ----- Common function input parameters:
// \param[in] position        The receiver position in any space that works with the shadow matrix.
// \param[in] positionWorld   The receiver position in world space.
// \param[in] positionScreen  The receiver position in screen space (in pixels)
//                            Same as float4 vPos : SV_POSITION.
// \param[in] texCoord        The shadow map texture coordinates of the current receiver position.
// \param[in] shadowMatrix    Converts the specified receiver position to the light projection space.
// \param[in] shadowMatrices  Converts the specified receiver position to the light projection space.
//                            This is an array with on matrix per CSM cascade.
// \param[in] shadowMap       The shadow map. Can be a texture atlas containing several maps.
// \param[in] shadowMapSize   The shadow map size (whole atlas) in texels (width, height).
// \param[in] shadowMapBounds The bounds of the shadow map inside the texture atlas.
//                            (left, top, right, bottom) in texture coordinates.
//                            (0, 0, 1, 1) if shadow map is not a texture atlas.
// \param[in] currentDepth    The relative depth ([0, 1]) of the current receiver position
//                            in the light space (= distance from the light source).
// \param[in] filterBilinear  'true' to apply bilinear filtering. 'false' to use the
//                            simple average of all samples.
//
// ----- Common function return values:
// All GetShadow functions return:
// \return The shadow factor in the range [0, 1].
//         0 means no shadow. 1 means full shadow.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_SHADOWMAP_FXH
#define DIGITALRUNE_SHADOWMAP_FXH

#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including ShadowMap.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

/// Declares the uniform const for a shadow map texture + sampler.
/// \param[in] name     The name of the shadow map texture constant.
/// \param[in] semantic The effect parameter semantic.
/// \remarks
/// Example: To declare ShadowMap and ShadowMapSampler for a directional light call
///   DECLARE_UNIFORM_SHADOWMAP(ShadowMap, DIRECTIONALLIGHTSHADOWMAP);
#define DECLARE_UNIFORM_SHADOWMAP(name, semantic) \
texture name : semantic; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = POINT; \
}


//-----------------------------------------------------------------------------
// General Functions
//-----------------------------------------------------------------------------

/// Samples the shadow map.
/// \return The sampled shadow map depth value.
/// \remarks
/// The texture coordinates are clamped to the given shadowMapBounds if following
/// symbol is defined:
/// #define CLAMP_TEXCOORDS_TO_SHADOW_MAP_RECTANGLE 1
float SampleShadowMap(sampler2D shadowMap, float2 texCoord, float2 offset, float4 shadowMapBounds)
{
  texCoord = texCoord + offset;
  
  // Clamp texture coordinates to the allowed rectangle of the texture atlas.
#if CLAMP_TEXCOORDS_TO_SHADOW_MAP_BOUNDS
  texCoord = clamp(texCoord, shadowMapBounds.xy, shadowMapBounds.zw);
#endif
  
  return tex2Dlod(shadowMap, float4(texCoord, 0, 0)).r;
}


/// Computes the texture coordinates in the shadow map for a given position.
/// \return The texture coordinates for the shadow map. z is the projected
///         depth of the position which can be used for directional lights.
float3 GetShadowTexCoord(float4 position, float4x4 shadowMatrix)
{
  // Transform position from world space to the projection space of the light source.
  position = mul(position, shadowMatrix);
  
  // Perspective divide.
  position.xyz /= position.w;
  
  // Convert position from light projection space to light texture space.
  position.xy = float2(0.5, -0.5) * position.xy + float2(0.5, 0.5);
  
  return position.xyz;
}


//-----------------------------------------------------------------------------
// Variance Shadow Maps (VSM)
//-----------------------------------------------------------------------------

/// Computes the depth moments that must be stored in a Variance Shadow Map (VSM).
/// \param[in] depth      The depth of the current pixel.
/// \param[in] applyBias  true if the variance (2nd moment) should consider the depth
///                       gradient of the current pixel. This costs a bit of performance
///                       but helps to remove surface acne. For most cases this is not
///                       necessary and applyBias can be false.
/// \return  The 1st and 2nd moment (average and variance) that must be stored in the
///          Variance Shadow Map.
float2 GetDepthMoments(float depth, bool applyBias)
{
  float2 moments;
  moments.x = depth;
  moments.y = depth * depth;
  
  if (applyBias)
  {
    // Consider the depth change at the current pixel to compute a better
    // variance to fight surface acne.
    float dx = ddx(depth);
    float dy = ddy(depth);
    moments.y = 0.25 * (dx * dx + dy * dy);
  }
  
  return moments;
}


/// Computes an upper bound for the probability that the given depth value is not occluded.
/// \param[in] moments      The first and second moments of the occluder depth (from the VSM).
/// \param[in] depth        The depth of the current pixel.
/// \param[in] minVariance  The minimum limit for the variance. The variance is clamped to
///                         this value.
/// \return The probability that a pixel at the given depth is not occluded.
float GetChebyshevUpperBound(float2 moments, float depth, float minVariance)
{
  // Compute variance.
  float variance = moments.y - moments.x * moments.x;
  variance = max(variance, minVariance + 0.0001f);
  
  // Compute upper bound for the probability.
  float delta = (depth - moments.x);
  float pMax = variance / (variance + delta * delta);
  
  // The inequality is only valid if depth >= moments.x.
  // If depth is <= moments.x the probability is p = 1 (100%).
  float p = (depth <= moments.x);
  return max(p, pMax);
}


//-----------------------------------------------------------------------------
// Normal Offset
//-----------------------------------------------------------------------------

/// Applies a slope-scaled normal offset to the specified position.
/// \param[in] position       The position.
/// \param[in] normal         The normal vector.
/// \param[in] light          The light vector (or light direction, sign does not matter).
/// \param[in] normalOffset   The max normal offset.
/// \return The position offset along the normal direction.
float3 ApplyNormalOffset(float3 position, float3 normal, float3 light, float normalOffset)
{
  float cosLight = abs(dot(normal, light));
  float slopeScale = saturate(1 - cosLight);
  return position + normal * slopeScale * normalOffset;
}


//-----------------------------------------------------------------------------
// Shadow Map Filtering
//-----------------------------------------------------------------------------

/// Determines whether a position is in the shadow using a single sample.
/// \remarks
/// This is the simplest GetShadow function. The result is either 0 or 1 - which
/// creates hard-edged, aliased shadows.
float GetShadow(float currentDepth, float2 texCoord, sampler2D shadowMap, float2 shadowMapSize, float4 shadowMapBounds)
{
  float occluderDepth = SampleShadowMap(shadowMap, texCoord, float2(0, 0), shadowMapBounds);
  return occluderDepth < currentDepth;
}


//-----------------------------------------------------------------------------
// CSM Functions
//-----------------------------------------------------------------------------

/// Converts texture coordinates of a single texture to the texture coordinates
/// for texture atlas containing several textures with equal size (horizontal layout).
/// \param[in] texCoord           The texture coordinates [0, 1].
/// \param[in] index              The texture index in the texture atlas.
/// \param[in] numberOfTextures   The number of textures in the atlas.
/// \return The correct texture coordinates to sample the texture atlas.
/// \remarks
/// The texture atlas consists of several textures in a horizontal row.
/// All 4 textures have the same size.
float2 ConvertToTextureAtlas(float2 texCoord, int index, int numberOfTextures)
{
  texCoord.x = ((float)index + texCoord.x) / (float)numberOfTextures;
  return texCoord;
}


/// Gets the bounds of a shadow cascade in the texture atlas.
/// \param[in] cascade            The cascade index.
/// \param[in] numberOfCascades   The number of cascades.
/// \param[in] shadowMapSize      The shadow map size (whole atlas) in texels (width, height).
/// \return The bounds (left, top, right, bottom) of the shadow cascade in texture atlas.
float4 GetShadowMapBounds(int cascade, int numberOfCascades, float2 shadowMapSize)
{
  float4 shadowMapBounds = float4(0, 0, 1, 1);
  shadowMapBounds.x = cascade / (float)numberOfCascades + 0.5 / shadowMapSize.x;
  shadowMapBounds.z = (cascade + 1) / (float)numberOfCascades - 0.5 / shadowMapSize.x;
  return shadowMapBounds;
}


/// Applies "shadow fog".
/// \param[in] isLastCascade      true if in last cascade or beyond.
/// \param[in] shadow       The shadow factor to which the "shadow fog" is applied.
/// \param[in] shadowTexCoord     The shadow map texture coordinates.
/// \param[in] currentDistance    The distance of the current position to the camera.
/// \param[in] maxDistance        The max distance of the shadows.
/// \param[in] fadeOutRange       The relative fade out range in [0, 1].
/// \param[in] fogShadowFactor    The shadow factor for objects beyond fogEnd.
/// \return The shadow factor with applied "shadow fog".
/// \remarks
/// Shadow fog has the effect that all pixel beyond are shadowed using a
/// constant shadow factor. Below fogStart shadows are drawn normally.
/// Between fogStart and fogEnd the lit and shadowed parts fade into the
/// shadow fog.
float ApplyShadowFog(float isLastCascade,
                     float shadow,
                     float3 shadowTexCoord,
                     float currentDistance,
                     float maxDistance,
                     float fadeOutRange,
                     float fogShadowFactor)
{
  // Fade out using only planar distance.
  // f = 0 ... outside shadow (in fog)
  // f = 1 ... in shadow (no fog).
  float f = saturate((maxDistance - currentDistance) / (maxDistance * fadeOutRange));
  return lerp(fogShadowFactor, shadow, f);
}


/// Chooses the Cascaded Shadow Map cascade by comparing cascade distances
/// and creates the texture coordinates for the shadow map.
/// \param[in]  cameraDistance    The camera z-distance.
/// \param[in]  cascadeDistances  The distances where the CSM cascades end.
/// \param[out] cascade           The cascade index.
/// \param[out] shadowTexCoord    The shadow map texture coordinates.
void ComputeCsmCascadeFast(float4 position,
                           float cameraDistance,
                           float4 cascadeDistances,
                           float4x4 shadowMatrices[4],
                           out int cascade,
                           out float3 shadowTexCoord)
{
  float3 greater = (cascadeDistances.xyz < cameraDistance);
  cascade = dot(greater, 1.0f);
  
  // For correct mipmap filtering:
  // cascade = FixCsmCascadeForMipMaps(cascade);
  
  shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
}
#endif
