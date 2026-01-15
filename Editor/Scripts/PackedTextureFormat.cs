namespace Rayforge.TexturePacker.Editor
{
    /// <summary>
    /// Supported texture packing formats for the Texture Packer.
    /// </summary>
    public enum PackedTextureFormat
    {
        //R8,        // 1 channel, 8-bit
        RHalf,     // 1 channel, 16-bit float
        RFloat,    // 1 channel, 32-bit float
        RGHalf,    // 2 channels, 16-bit float
        RGFloat,   // 2 channels, 32-bit float
        RGBA32,    // 4 channels, 8-bit per channel
        RGBAHalf,  // 4 channels, 16-bit float per channel
        RGBAFloat  // 4 channels, 32-bit float per channel
    }
}
