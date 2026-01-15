# Rayforge Texture Packer

A Unity editor tool for packing multiple source textures into a single optimized texture. Supports per-channel configuration, real-time preview, and flexible export options.

## Features

- **Channel-based packing**: Combine R, G, B, and A channels from different source textures
- **Per-channel control**: Individual settings for source channel, inversion, multipliers, and force white
- **Real-time preview**: Visualize packed results before export with channel-specific preview modes
- **Preset system**: Save, load, and revert texture packing configurations
- **Flexible output**: Configurable resolution (16-8192px), power-of-2 option, and multiple texture formats
- **Compute & raster blitting**: Automatic fallback for optimal performance

## Requirements

- **Unity 6000.0.58f2 or higher (tested)**

Older versions may cause compatibility issues.

## Installation

### Via Unity Package Manager

#### Step 1: Install Rayforge Core Library

1. Open Unity and navigate to `Window > Package Manager`
2. Click the **+** icon in the top-left corner
3. Select `Add package from git URL…`
4. Enter the following URL:
   ```
   https://github.com/rayforge-math/RayforgeUnityLibraryCore.git
   ```
5. Click **Add** and wait for the installation to complete

#### Step 2: Install Texture Packer

1. In the Package Manager, click the **+** icon again
2. Select `Add package from git URL…`
3. Enter the following URL:
   ```
   https://github.com/rayforge-math/RayforgeUnityLibraryCore.git#Blitter
   ```
4. Click **Add**

The Texture Packer will be downloaded and added to your project.

> **Important:** The Rayforge Core Library must be installed first, as the Texture Packer depends on it.

## Setup

The Texture Packer automatically checks for required dependencies on each launch:

- **Rayforge Core Library** - Installed automatically if missing
- **Scriptable Render Pipeline (SRP)** - Not installed automatically, notice displayed

### Manual

1. Navigate to `Window > Rayforge > Texture Packer > Installer`
2. The Installer window will open
3. Verify all required dependencies show **"Installed"** status
4. Click **Install Dependencies** to install any missing required packages

## Usage

### Opening the Tool

Navigate to `Tools > Rayforge > Texture Packer` in the Unity menu bar.

### Working with Presets

**Create a new preset:**
1. Configure your desired settings in the Texture Packer window
2. Click **Save As** to create a new preset asset
3. Choose a location and name for your preset

**Load an existing preset:**
- Use the object field at the top of the window to select a saved preset

**Modify and save:**
- Make changes to the current preset
- Click **Save** to overwrite the active preset
- Click **Revert** to discard changes and reload the original preset

### Configuring Channels

For each of the four output channels (R, G, B, A):

1. **Name**: Label the channel for easy identification
2. **Source Texture**: Assign the input texture
3. **Source Channel**: Select which channel (R, G, B, or A) to read from the source
4. **Force White**: Override source data with white (useful for masks)
5. **Invert**: Invert the channel values (1 - value)
6. **Multiplier**: Scale the channel values (0.0 - 2.0)
7. **Clear**: Reset the channel to default settings

Click **Clear All** to reset all four channels at once.

### Output Settings

- **Power of 2**: Automatically snap resolution to nearest power of 2
- **Resolution**: Set output texture size (16-8192 pixels)
- **Format**: Choose texture format (RGBA32, RGBAHalf, etc.)

### Preview and Export

**Preview modes:**
- **All**: View all channels combined (final result)
- **Red/Green/Blue/Alpha**: View individual channels for debugging

**Actions:**
- **Create Preview**: Generate a preview without exporting
- **Pack Texture**: Generate and export the final packed texture

The tool will prompt you to choose an export location and filename when packing.

### Example Workflow

1. Create a new preset or load an existing one
2. Assign source textures to each channel (e.g., Metallic to R, Occlusion to G, Detail Mask to B)
3. Configure per-channel settings (invert roughness, multiply AO, etc.)
4. Set output resolution and format
5. Click **Create Preview** to verify the result
6. Adjust settings if needed
7. Click **Pack Texture** to export the final texture

## Tips

- Use **Force White** for channels that should be fully opaque or enabled
- The **Multiplier** is useful for adjusting intensity of maps like ambient occlusion
- **Invert** is commonly used for converting smoothness to roughness maps
- Preview individual channels to verify correct source data before packing
- Save presets for common packing configurations (e.g., PBR material maps)

## Support

For issues or questions, please visit the [GitHub repository](https://github.com/rayforge-math/RayforgeUnityLibraryCore).

## License

See the LICENSE file in the repository for licensing information.