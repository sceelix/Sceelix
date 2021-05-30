//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com/
// Copyright (C) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file Encoding.fxh
/// Functions to encode/decode values.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_ENCODING_FXH
#define DIGITALRUNE_ENCODING_FXH



//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

/// Declares the uniform const for the normals fitting texture + sampler.
/// \remarks
/// The normals fitting textures is a lookup texture used to encode normals in
/// EncodeNormalBestFit().
#define DECLARE_UNIFORM_NORMALSFITTINGTEXTURE \
texture NormalsFittingTexture : NORMALSFITTINGTEXTURE; \
sampler NormalsFittingSampler = sampler_state \
{ \
  Texture = <NormalsFittingTexture>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = POINT; \
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Encodes the given color to RGBM format.
/// \param[in] color    The original color.
/// \param[in] maxValue The max value, e.g. 6 (if color is gamma corrected) =
///                     6 ^ 2.2 (if color is in linear space).
/// \return The color in RGBM format.
/// \remarks
/// The input color can be in linear space or in gamma space. It is recommended
/// convert the color to gamma space before encoding as RGBM.
/// See http://graphicrants.blogspot.com/2009/04/rgbm-color-encoding.html.
float4 EncodeRgbm(float3 color, float maxValue)
{
  float4 rgbm;
  color /= maxValue;
  rgbm.a = saturate(max(max(color.r, color.g), max(color.b, 1e-6)));
  rgbm.a = ceil(rgbm.a * 255.0) / 255.0;
  rgbm.rgb = color / rgbm.a;
  return rgbm;
}


/// Decodes the given color from RGBM format.
/// \param[in] rgbm      The color in RGBM format.
/// \param[in] maxValue  The max value, e.g. 6 (if color is gamma corrected) =
///                      6 ^ 2.2 (if color is in linear space).
/// \return The original RGB color (can be in linear or gamma space).
float3 DecodeRgbm(float4 rgbm, float maxValue)
{
  return maxValue * rgbm.rgb * rgbm.a;
}


/// Encodes a normal vector in 3 8-bit channels.
/// \param[in] normal                 The normal vector.
/// \param[in] normalsFittingSampler  The lookup texture for normal fitting.
/// \return The normal encoded for storage in an RGB texture (3 x 8 bit).
half3 EncodeNormalBestFit(half3 normal, sampler normalsFittingSampler)
{
  // Best-fit normal encoding as in "CryENGINE 3: Reaching the Speed of Light"
  // by Anton Kaplanyan (Crytek). See http://advances.realtimerendering.com/s2010/index.html
  
  // Renormalize (needed if any blending or interpolation happened before).
  normal.rgb = (half3)normalize(normal.rgb);
  // Get unsigned normal for cubemap lookup. (Note, the full float precision is required.)
  half3 unsignedNormal = abs(normal.rgb);
  // Get the main axis for cubemap lookup.
  half maxNAbs = max(unsignedNormal.z, max(unsignedNormal.x, unsignedNormal.y));
  // Get texture coordinates in a collapsed cubemap.
  float2 texcoord = unsignedNormal.z < maxNAbs ? (unsignedNormal.y < maxNAbs ? unsignedNormal.yz : unsignedNormal.xz) : unsignedNormal.xy;
  texcoord = texcoord.x < texcoord.y ? texcoord.yx : texcoord.xy;
  texcoord.y /= texcoord.x;
  // Fit normal into the edge of unit cube.
  normal.rgb /= maxNAbs;
  // Look-up fitting length and scale the normal to get the best fit.
  half fittingScale = (half)tex2D(normalsFittingSampler, texcoord).a;
  // Scale the normal to get the best fit.
  normal.rgb *= fittingScale;
  // Squeeze back to unsigned.
  normal.rgb = normal.rgb * 0.5h + 0.5h;
  return normal;
}


/// Decodes a normal that was encoded with EncodeNormalBestFit().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalBestFit(half4 encodedNormal)
{
  return (half3)normalize(encodedNormal.xyz * 2 - 1);
}


/// Encodes a specular power (to be stored as unsigned byte).
/// \param[in] specularPower   The specular power.
/// \return The encoded specular power [0, 1].
float EncodeSpecularPower(float specularPower)
{
  return log2(specularPower + 0.0001f) / 17.6f; // max = 200000
}


/// Decodes the specular power (stored as unsigned byte).
/// \param[in] encodedSpecularPower    The encoded specular power [0, 1].
/// \return The original specular power.
float DecodeSpecularPower(float encodedSpecularPower)
{
  return exp2(encodedSpecularPower * 17.6f);
}
#endif
