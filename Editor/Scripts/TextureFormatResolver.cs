using System;
using UnityEngine;

namespace Rayforge.TexturePacker.Editor
{
    /// <summary>
    /// Resolves Unity RenderTextureFormat and TextureFormat from a PackedTextureFormat.
    /// Also exposes whether the format supports RandomWrite for compute shaders.
    /// </summary>
    public static class TextureFormatResolver
    {
        public static RenderTextureFormat ToRenderTextureFormat(PackedTextureFormat format)
        {
            return format switch
            {
                //PackedTextureFormat.R8 => RenderTextureFormat.R8,
                PackedTextureFormat.RHalf => RenderTextureFormat.RHalf,
                PackedTextureFormat.RFloat => RenderTextureFormat.RFloat,
                PackedTextureFormat.RGHalf => RenderTextureFormat.RGHalf,
                PackedTextureFormat.RGFloat => RenderTextureFormat.RGFloat,
                PackedTextureFormat.RGBA32 => RenderTextureFormat.ARGB32,
                PackedTextureFormat.RGBAHalf => RenderTextureFormat.ARGBHalf,
                PackedTextureFormat.RGBAFloat => RenderTextureFormat.ARGBFloat,
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported format: {format}")
            };
        }

        public static TextureFormat ToTexture2DFormat(PackedTextureFormat format)
        {
            return format switch
            {
                //PackedTextureFormat.R8 => TextureFormat.R8,
                PackedTextureFormat.RHalf => TextureFormat.RHalf,
                PackedTextureFormat.RFloat => TextureFormat.RFloat,
                PackedTextureFormat.RGHalf => TextureFormat.RGHalf,
                PackedTextureFormat.RGFloat => TextureFormat.RGFloat,
                PackedTextureFormat.RGBA32 => TextureFormat.RGBA32,
                PackedTextureFormat.RGBAHalf => TextureFormat.RGBAHalf,
                PackedTextureFormat.RGBAFloat => TextureFormat.RGBAFloat,
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported format: {format}")
            };
        }

        public static bool SupportsRandomWrite(PackedTextureFormat format)
        {
            return format switch
            {
                //PackedTextureFormat.R8 => false,
                PackedTextureFormat.RHalf => true,
                PackedTextureFormat.RFloat => true,
                PackedTextureFormat.RGHalf => true,
                PackedTextureFormat.RGFloat => true,
                PackedTextureFormat.RGBA32 => false,
                PackedTextureFormat.RGBAHalf => true,
                PackedTextureFormat.RGBAFloat => true,
                _ => false
            };
        }

        public static int GetChannelCount(PackedTextureFormat format)
        {
            return format switch
            {
                //PackedTextureFormat.R8 => 1,
                PackedTextureFormat.RHalf => 1,
                PackedTextureFormat.RFloat => 1,
                PackedTextureFormat.RGHalf => 2,
                PackedTextureFormat.RGFloat => 2,
                PackedTextureFormat.RGBA32 => 4,
                PackedTextureFormat.RGBAHalf => 4,
                PackedTextureFormat.RGBAFloat => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported format: {format}")
            };
        }
    }
}
