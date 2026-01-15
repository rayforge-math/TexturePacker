using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rayforge.TexturePacker.Editor
{
    public static class TextureExporter
    {
        /// <summary>
        /// Exports a RenderTexture to disk as PNG (8-bit) or EXR (float).
        /// Opens a save file dialog.
        /// </summary>
        /// <param name="rt">RenderTexture to export.</param>
        /// <param name="format">Target export format. PNG for 8-bit, EXR for float formats.</param>
        public static void ExportRenderTexture(RenderTexture rt, PackedTextureFormat format)
        {
            if (rt == null)
            {
                Debug.LogWarning("No RenderTexture provided for export.");
                return;
            }

            string defaultExt = (/*format == PackedTextureFormat.R8 ||*/ format == PackedTextureFormat.RGBA32) ? "png" : "exr";
            string path = EditorUtility.SaveFilePanel("Export Packed Texture", "", "PackedTexture." + defaultExt, defaultExt);
            if (string.IsNullOrEmpty(path))
                return;

            TextureFormat texFormat = TextureFormat.RGBA32;
            bool isFloat = false;

            switch (format)
            {
                //case PackedTextureFormat.R8:
                case PackedTextureFormat.RGBA32:
                    texFormat = TextureFormat.RGBA32;
                    isFloat = false;
                    break;
                case PackedTextureFormat.RHalf:
                case PackedTextureFormat.RFloat:
                case PackedTextureFormat.RGHalf:
                case PackedTextureFormat.RGFloat:
                case PackedTextureFormat.RGBAHalf:
                case PackedTextureFormat.RGBAFloat:
                    texFormat = TextureFormat.RGBAFloat;
                    isFloat = true;
                    break;
                default:
                    texFormat = TextureFormat.RGBA32;
                    break;
            }

            Texture2D tex = new Texture2D(rt.width, rt.height, texFormat, false, true);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;

            byte[] bytes;
            if (!isFloat)
            {
                bytes = tex.EncodeToPNG();
            }
            else
            {
                bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            }

            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            Debug.Log($"Texture exported to: {path}");
        }
    }
}
