using System;
using UnityEngine;

using Rayforge.Core.Rendering.Blitter;

namespace Rayforge.TexturePacker.Editor
{
    /// <summary>
    /// Represents a single channel contribution from a source texture
    /// to an output channel in a packed texture.
    /// </summary>
    [Serializable]
    public struct ChannelContribution
    {
        /// <summary>
        /// Friendly name for the channel (optional, used in UI or presets).
        /// </summary>
        public string name;

        /// <summary>
        /// The texture to read the channel from. 
        /// If null, this contribution is considered inactive.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// The source channel of <see cref="texture"/> to use for this contribution.
        /// Default is <see cref="Channel.None"/> for an inactive contribution.
        /// </summary>
        public Channel source;

        /// <summary>
        /// Whether to invert the selected channel when writing to the output.
        /// </summary>
        public bool invert;

        /// <summary>
        /// Multiplier applied to the selected channel.
        /// Default is 1.0f.
        /// </summary>
        public float multiply;

        /// <summary>
        /// If true, this channel outputs a constant 1 (white) instead of reading from a texture.
        /// Overrides <see cref="texture"/> and <see cref="source"/>.
        /// </summary>
        public bool forceWhite;

        /// <summary>
        /// Returns a disabled contribution with no source channel and a multiplier of 1.
        /// </summary>
        public static ChannelContribution Default(string name) =>
            new ChannelContribution
            {
                name = string.IsNullOrEmpty(name) ? "None" : name,
                source = Channel.None,
                multiply = 1f
            };

        /// <summary>
        /// Returns true if this contribution actually provides data for the blit operation.
        /// A contribution is considered active if it either has a valid texture and channel
        /// or if it is forced to output white (forceWhite = true).
        /// Use this to determine whether this channel will have an effect on the packed texture.
        /// </summary>
        public bool IsActive => (texture != null && source != Channel.None) || forceWhite;

        /// <summary>
        /// Returns true if the configuration of this contribution is logically valid.
        /// A contribution is valid if:
        /// - It is set to force white (forceWhite = true), OR
        /// - The source channel is None (no contribution), OR
        /// - It has a valid texture assigned for the selected source channel.
        /// Use this for editor validation or preset checking to ensure the configuration
        /// makes sense, independent of whether it contributes anything visually.
        /// </summary>
        public bool IsValid()
        {
            if (forceWhite) return true;
            if (source == Channel.None) return true;
            return texture != null;
        }

        /// <summary>
        /// Returns a safe display name for this contribution.
        /// If the 'name' field is null or empty, returns "None" as a fallback.
        /// Useful for UI labels or presets to avoid empty or null strings.
        /// </summary>
        public string SafeName => string.IsNullOrEmpty(name) ? "None" : name;
    }
}
