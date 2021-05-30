//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com
// Copyright (c) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file Noise.fxh
/// GPU-based noise functions.
///
/// If the Dither function results are used for screen door transparency, use
/// it like this:
///   clip(Alpha - Dither(pixel pos));
///
/// When using screen door transparency for blending LODs, one LOD needs to use
///   clip(Alpha - Dither(pixel pos));
/// and the other LOD needs to use the inverse pattern
///   clip(Alpha - (1 - Dither(pixel pos)));
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_NOISE_FXH
#define DIGITALRUNE_NOISE_FXH

/// Returns a value in the range [0, 1] of a regular 4x4 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither4x4(float2 p)
{
  static const float4x4 DitherMatrix =
  {
     1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
     4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
  };
  return DitherMatrix[p.x % 4][p.y % 4];
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Three values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise1(float2 p)
{
  return frac(sin(dot(p.xy, float2(12.9898, 78.233)))* 43758.5453);
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Two values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise2(float2 p)
{
  // The noise is a modification of the noise algorithm of Pat 'Hawthorne' Shearon.
  
  float a = p.x * p.y * 50000.0;
  a = fmod(a, 13);
  a = a * a;
  
  float randomA = fmod(a, 0.01) * 100;
  return randomA;
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Three values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise2(float3 p)
{
  // The noise is a modification of the noise algorithm of Pat 'Hawthorne' Shearon.
  
  float a = p.x * p.y * 50000.0;
  a = fmod(a, 13);
  a = a * a;
  
  float z = abs(p.z);  // z should be around [0, 1].
  float b = a * z + z;
  float randomB = fmod(b, 0.01) * 100;
  
  return abs(randomB);
}
#endif
