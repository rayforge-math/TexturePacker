using Rayforge.Core.Rendering.Blitter;
using UnityEngine;

using Rayforge.Core.Rendering.Blitter.Helpers;

namespace Rayforge.TexturePacker.Editor
{
    /// <summary>
    /// Serializable preset describing how source textures are packed into RGBA channels.
    /// Can be stored as an asset and reused across projects.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TexturePackerPreset",
        menuName = Bootstrap.PackageInfo.CompanyName + "/" + Bootstrap.PackageInfo.PackageTitle + "/Preset"
    )]
    public class TexturePackerPreset : ScriptableObject
    {
        /// <summary>
        /// Per-channel contribution definitions (RGBA).
        /// </summary>
        public ChannelContribution[] contributions = new ChannelContribution[4]
        {
            ChannelContribution.Default(Channel.R.ToFriendlyString()),
            ChannelContribution.Default(Channel.G.ToFriendlyString()),
            ChannelContribution.Default(Channel.B.ToFriendlyString()),
            ChannelContribution.Default(Channel.A.ToFriendlyString())
        };

        /// <summary>
        /// Output format of the packed texture.
        /// </summary>
        public PackedTextureFormat textureFormat = PackedTextureFormat.RGBA32;

        /// <summary>
        /// Base resolution of the output texture.
        /// </summary>
        public int resolution = 512;

        /// <summary>
        /// Whether the resolution is forced to a power-of-two value.
        /// </summary>
        public bool powerOfTwo = true;

        /// <summary>
        /// Creates a deep copy of this preset (used for preview or duplicating presets).
        /// </summary>
        public TexturePackerPreset DeepCopy()
        {
            var copy = CreateInstance<TexturePackerPreset>();
            copy.textureFormat = textureFormat;
            copy.resolution = resolution;
            copy.powerOfTwo = powerOfTwo;

            copy.contributions = new ChannelContribution[contributions.Length];
            for (int i = 0; i < contributions.Length; i++)
                copy.contributions[i] = contributions[i];

            return copy;
        }
    }
}
