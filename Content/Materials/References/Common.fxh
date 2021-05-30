//-----------------------------------------------------------------------------
// DigitalRune Graphics - http://www.digitalrune.com/
// Copyright (C) DigitalRune GmbH. All rights reserved.
//-----------------------------------------------------------------------------
//
/// \file Common.fxh
/// Frequently used constants and functions.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_COMMON_FXH
#define DIGITALRUNE_COMMON_FXH


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// The gamma value.
// Use 2.0 for approximate gamma (default) and 2.2 for exact gamma.
#ifndef DR_GAMMA
#define DR_GAMMA 2.0
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Converts a color given in non-linear gamma space to linear space.
/// \param[in] colorGamma  The color in non-linear gamma space.
/// \return The color in linear space.
float3 FromGamma(float3 colorGamma)
{
  return pow(colorGamma, DR_GAMMA);
}


/// Converts a color given in linear space to non-linear gamma space.
/// \param[in] colorLinear  The color in linear space.
/// \return The color in non-linear gamma space.
float3 ToGamma(float3 colorLinear)
{
  return pow(colorLinear, 1 / DR_GAMMA);
}


/// Transforms a screen space position to standard projection space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The screen space position in "pixels".
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The position in projection space.
float2 ScreenToProjection(float2 position, float2 viewportSize)
{
#if !SM4
  // Subtract a half pixel so that the edge of the primitive is between screen pixels.
  // Thus, the first texel lies exactly on the first pixel.
  // See also http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
  // for a good description of this DirectX 9 problem.
  position -= 0.5;
#endif
  
  // Now transform screen space coordinate into projection space.
  // Screen space: Left top = (0, 0), right bottom = (ScreenSize.x - 1, ScreenSize.y - 1).
  // Projection space: Left top = (-1, 1), right bottom = (1, -1).
  
  // Transform into the range [0, 1] x [0, 1].
  position /= viewportSize;
  // Transform into the range [0, 2] x [-2, 0]
  position *= float2(2, -2);
  // Transform into the range [-1, 1] x [1, -1].
  position -= float2(1, -1);
  
  return position;
}


/// A default vertex shader function calling ScreenToProjection().
/// \param[in,out] texCoord     In/out: The texture coordinates (unchanged).
/// \param[in,out] position     In: The position in screen space.
///                             Out: The position in projection space.
/// \param[in]     viewportSize The viewport size in pixels, e.g. (1280, 720).
void VSScreenSpaceDraw(inout float2 texCoord : TEXCOORD0,
                       inout float4 position : SV_POSITION,
                       uniform const float2 viewportSize)
{
  position.xy = ScreenToProjection(position.xy, viewportSize);
}


/// Transforms a position from standard projection space to screen space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The position in projection space.
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The screen space position in texture coordinates ([0, 1] range).
float2 ProjectionToScreen(float4 position, float2 viewportSize)
{
  // Perspective divide:
  position.xy = position.xy / position.w;
  
  // Convert from range [-1, 1] x [1, -1] to [0, 1] x [0, 1].
  position.xy = float2(0.5, -0.5) * position.xy + float2(0.5, 0.5);
  
  // The position (0, 0) is the center of the first screen pixel. We have
  // to add half a texel to sample the center of the first texel.
#if !SM4
  position.xy += 0.5f / viewportSize;
#endif
  
  return position.xy;
}


//-----------------------------------------------------------------------------
// Texture Mapping
//-----------------------------------------------------------------------------

/// Computes the current mip map level for a texture.
/// \param[in] texCoord      The texture coordinates.
/// \param[in] textureSize   The size of the texture in pixels (width, height).
/// \return The mip map level.
float MipLevel(float2 texCoord, float2 textureSize)
{
  // Note: This code is taken from Shader X5 - Practical Parallax Occlusion Mapping
  // which is similar to the DirectX 2009 Parallax Occlusion Mapping sample code.
  
  // Compute mip map level for texture.
  float2 scaledTexCoord = texCoord * textureSize;
  
  // Compute partial derivatives of the texture coordinates with respect to screen
  // coordinates. The derivatives are an approximation of the pixel's size in texture
  // space.
  float2 dxSize = ddx(scaledTexCoord);
  float2 dySize = ddy(scaledTexCoord);
  
  // Find max of change in u and v across quad: Compute du and dv magnitude across quad.
  float2 dTexCoord = dxSize * dxSize + dySize * dySize;
  float maxTexCoordDelta = max(dTexCoord.x, dTexCoord.y);
  
  // The mip map level k for a given compression value d is such that
  //   2^k <= d < 2^(k+1)
  // Or:
  //   k = floor(log2(d))
  
  // Compute the current mip map level.
  // (The factor 0.5 is effectively computing a square root before log.)
  return max(0.5 * log2(maxTexCoordDelta), 0);
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinear(sampler textureSampler, float2 texCoord, float2 textureSize)
{
  float2 texelSize = 1.0 / textureSize;
  texCoord -= 0.5 * texelSize;
  float4 s00 = tex2D(textureSampler, texCoord);
  float4 s10 = tex2D(textureSampler, texCoord + float2(texelSize.x, 0));
  float4 s01 = tex2D(textureSampler, texCoord + float2(0, texelSize.y));
  float4 s11 = tex2D(textureSampler, texCoord + texelSize);
  
  float2 texelpos = textureSize * texCoord;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates for tex2Dlod.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinearLod(sampler textureSampler, float4 texCoord, float2 textureSize)
{
  float2 texelSize = 1.0 / textureSize;
  texCoord.xy -= 0.5 * texelSize;
  float4 s00 = tex2Dlod(textureSampler, float4(texCoord.xy, texCoord.z, texCoord.w));
  float4 s10 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(texelSize.x, 0), texCoord.z, texCoord.w));
  float4 s01 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(0, texelSize.y), texCoord.z, texCoord.w));
  float4 s11 = tex2Dlod(textureSampler, float4(texCoord.xy + texelSize, texCoord.z, texCoord.w));
  
  float2 texelpos = textureSize * texCoord.xy;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}


//-----------------------------------------------------------------------------
// Normal Mapping
//-----------------------------------------------------------------------------

/// Gets the normal vector from a standard normal texture (no special encoding).
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormal(sampler normalSampler, float2 texCoord)
{
  float3 normal = tex2D(normalSampler, texCoord).xyz * 255/128 - 1;
  normal = normalize(normal);
  return normal;
}


/// Gets the normal vector from a normal texture which uses DXT5nm encoding.
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormalDxt5nm(sampler normalSampler, float2 texCoord)
{
  float3 normal;
  normal.xy = tex2D(normalSampler, texCoord).ag * 255/128 - 1;
  normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));
  return normal;
}


/// Computes the cotangent frame.
/// \param[in] n   The (normalized) normal vector.
/// \param[in] p   The position.
/// \param[in] uv  The texture coordinates.
/// \return The cotangent frame.
/// \remarks
/// For reference see http://www.thetenthplanet.de/archives/1180.
/// Example: To convert a normal n from a normal map to world space.
///  float3x3 cotangentFrame = CotangentFrame(normalWorld, positionWorld, texCoord);
///  nWorld = mul(n, cotangentFrame);
float3x3 CotangentFrame(float3 n, float3 p, float2 uv)
{
  // Get edge vectors of the pixel triangle.
  float3 dp1 = ddx(p);
  float3 dp2 = ddy(p);
  float2 duv1 = ddx(uv);
  float2 duv2 = ddy(uv);
  
  // ----- Original
  // Solve the linear system.
  //float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
  //float3x3 inverseM = Invert(M);
  //float3 T = mul(inverseM, float3(duv1.x, duv2.x, 0));
  //float3 B = mul(inverseM, float3(duv1.y, duv2.y, 0));
  
  // ----- Optimized
  float3 dp2perp = cross(n, dp2);
  float3 dp1perp = cross(dp1, n);
  float3 t = dp2perp * duv1.x + dp1perp * duv2.x;
  float3 b = dp2perp * duv1.y + dp1perp * duv2.y;
  
  // Construct a scale-invariant frame.
  float invmax = rsqrt(max(dot(t, t), dot(b, b)));
  return float3x3(t * invmax, b * invmax, n);
}


//-----------------------------------------------------------------------------
// Util
//-----------------------------------------------------------------------------

/// Computes where a ray hits a sphere (which is centered at the origin).
/// \param[in]  rayOrigin    The start position of the ray.
/// \param[in]  rayDirection The normalized direction of the ray.
/// \param[in]  radius       The radius of the sphere.
/// \param[out] enter        The ray parameter where the ray enters the sphere.
///                          0 if the ray is already in the sphere.
/// \param[out] exit         The ray parameter where the ray exits the sphere.
/// \return  0 or a positive value if the ray hits the sphere. A negative value
///          if the ray does not touch the sphere.
float HitSphere(float3 rayOrigin, float3 rayDirection, float radius, out float enter, out float exit)
{
  // Solve the equation:  ||rayOrigin + distance * rayDirection|| = r
  //
  // This is a straight-forward quadratic equation:
  //   ||O + d * D|| = r
  //   =>  (O + d * D)² = r²  where V² means V.V
  //   =>  d² * D² + 2 * d * (O.D) + O² - r² = 0
  // D² is 1 because the rayDirection is normalized.
  //   =>  d = -O.D + sqrt((O.D)² - O² + r²)
  
  float OD = dot(rayOrigin, rayDirection);
  float OO = dot(rayOrigin, rayOrigin);
  float radicand = OD * OD - OO + radius * radius;
  enter = max(0, -OD - sqrt(radicand));
  exit = -OD + sqrt(radicand);
  
  return radicand;  // If radicand is negative then we do not have a result - no hit.
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float3 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float4 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Returns a linear interpolation betwenn 0 and 1 if x is in the range [min, max].
/// This does the same as the HLSL intrinsic function smoothstep() - but without
/// a smooth curve.
///  min  The minimum range of the x parameter.
///  max  The maximum range of the x parameter.
///  x    The specified value to be interpolated.
///  Returns 0 if x is less than min;
///  1 if x is greater than max;
///  otherwise, a value between 0 and 1 if x is in the range [min, max].
float LinearStep(float min, float max, float x)
{
  float y = (x - min) / (max - min);
  return clamp(y, 0, 1);
}
#endif
