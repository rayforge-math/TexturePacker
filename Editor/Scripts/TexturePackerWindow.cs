using System;
using UnityEngine;
using UnityEditor;

using Rayforge.Core.Rendering.Blitter;
using Rayforge.Core.Rendering.Blitter.Helpers;
using Rayforge.Core.Rendering.Collections;
using UnityEngine.Rendering;

namespace Rayforge.TexturePacker.Editor
{
    /// <summary>
    /// Editor window for packing multiple source textures into a single output texture.
    /// Supports per-channel workingPreset.contributions, previewing, and exporting.
    /// </summary>
    public class TexturePackerWindow : EditorWindow
    {
        private const string Title = Bootstrap.PackageInfo.PackageTitle;

        private const string WindowTitle = Title;

        private const string LastPresetKey = Title + ".LastPreset";

        [SerializeField]
        private TexturePackerPreset activePreset;

        [SerializeField]
        private TexturePackerPreset workingPreset;

        /// <summary>
        /// Scroll position for the editor GUI.
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// Preview mode for showing a single channel or all channels.
        /// </summary>
        private enum PreviewMode { Red, Green, Blue, Alpha, All }
        private PreviewMode previewMode = PreviewMode.All;

        /// <summary>
        /// Ping-pong buffer used for packing and previewing.
        /// </summary>
        private PingPongBuffer<RenderTexture> packedTextures = new(null, null);

        /// <summary>
        /// Opens the Texture Packer window in Unity.
        /// </summary>
        [MenuItem("Tools/" + Bootstrap.PackageInfo.CompanyName + "/" + WindowTitle)]
        public static void ShowWindow()
        {
            var window = GetWindow<TexturePackerWindow>(true, WindowTitle);
            window.minSize = new Vector2(350, 560);
        }

        /// <summary>
        /// Draws the main GUI.
        /// </summary>
        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            DrawPresetBar();
            GUILayout.Space(8);

            DrawInputs();
            GUILayout.Space(10);

            DrawOutputHeader();
            GUILayout.Space(10);

            DrawPreviewWithButtons();
            GUILayout.Space(10);

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Initializes the packer textures when the window is enabled.
        /// </summary>
        private void OnEnable()
        {
            RestoreLastPreset();
            EnsureWorkingPreset();
            ReAllocPackerTextures();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            CleanupRenderTextures();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                ReAllocPackerTextures();
            }
        }

        /// <summary>
        /// Draws the preset toolbar at the top of the window.
        /// This bar is responsible for selecting, saving, duplicating,
        /// and reverting Texture Packer presets.
        /// </summary>
        private void DrawPresetBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Preset field
            EditorGUI.BeginChangeCheck();
            var newPreset = (TexturePackerPreset)EditorGUILayout.ObjectField(
                activePreset,
                typeof(TexturePackerPreset),
                false,
                GUILayout.MinWidth(160));

            if (EditorGUI.EndChangeCheck() && newPreset != null)
            {
                LoadPreset(newPreset);
            }

            GUILayout.FlexibleSpace();

            // Save
            using (new EditorGUI.DisabledScope(activePreset == null))
            {
                if (GUILayout.Button("Save", GUILayout.Width(60)))
                {
                    SavePreset();
                }
            }

            // Save As
            if (GUILayout.Button("Save As", GUILayout.Width(70)))
            {
                SavePresetAs();
            }

            // Reset
            using (new EditorGUI.DisabledScope(activePreset == null))
            {
                if (GUILayout.Button("Revert", GUILayout.Width(60)))
                {
                    LoadPreset(activePreset);
                }
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws per-channel input settings (texture, source channel, inversion, multiplier, force white).
        /// </summary>
        private void DrawInputs()
        {
            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                workingPreset.contributions[i].name = GUILayout.TextField(workingPreset.contributions[i].name);

                workingPreset.contributions[i].texture = (Texture2D)EditorGUILayout.ObjectField(
                    "Source Texture",
                    workingPreset.contributions[i].texture,
                    typeof(Texture2D),
                    false
                );

                workingPreset.contributions[i].source = (Channel)EditorGUILayout.EnumPopup("Source Channel", workingPreset.contributions[i].source);
                workingPreset.contributions[i].forceWhite = EditorGUILayout.Toggle("Force White", workingPreset.contributions[i].forceWhite);
                workingPreset.contributions[i].invert = EditorGUILayout.Toggle("Invert", workingPreset.contributions[i].invert);
                workingPreset.contributions[i].multiply = EditorGUILayout.Slider("Multiplier", workingPreset.contributions[i].multiply, 0f, 2f);

                if (GUILayout.Button("Clear"))
                {
                    workingPreset.contributions[i] = ChannelContribution.Default(((Channel)i).ToFriendlyString());
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            if (GUILayout.Button("Clear All"))
            {
                ClearContributions();
            }
            GUILayout.Space(5);
        }

        /// <summary>
        /// Draws the output settings section (workingPreset.resolution, format, power-of-2 option).
        /// </summary>
        private void DrawOutputHeader()
        {
            GUILayout.Label("Output Settings", EditorStyles.boldLabel);

            workingPreset.powerOfTwo = EditorGUILayout.Toggle("Power of 2", workingPreset.powerOfTwo);

            workingPreset.resolution = EditorGUILayout.IntSlider("Resolution", workingPreset.resolution, 16, 8192);
            if (workingPreset.powerOfTwo)
            {
                workingPreset.resolution = Mathf.NextPowerOfTwo(Mathf.Max(1, workingPreset.resolution));
            }

            workingPreset.textureFormat = (PackedTextureFormat)EditorGUILayout.EnumPopup("Format", workingPreset.textureFormat);

            GUILayout.Space(5);
        }

        private void DrawPreviewWithButtons()
        {
            GUILayout.Label("Preview", EditorStyles.boldLabel);

            previewMode = (PreviewMode)EditorGUILayout.EnumPopup("Show Channels", previewMode);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            if (packedTextures.Second != null && packedTextures.Second.IsCreated())
            {
                EditorGUI.DrawPreviewTexture(previewRect, packedTextures.Second);
            }
            else
            {
                EditorGUI.DrawPreviewTexture(previewRect, Texture2D.blackTexture);
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Space(50);

            if (GUILayout.Button("Create Preview", GUILayout.Width(120), GUILayout.Height(40)))
            {
                CreatePackedTexture(false);
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Pack Texture", GUILayout.Width(120), GUILayout.Height(40)))
            {
                CreatePackedTexture(true);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates the packed texture using compute or raster blit.
        /// </summary>
        /// <param name="finalize">If true, the texture will be exported.</param>
        private void CreatePackedTexture(bool finalize)
        {
            ReAllocPackerTextures();
            PerformComputePacking(finalize, PerformRasterBlit);
        }

        /// <summary>
        /// Performs a compute-based packing of the current workingPreset.contributions into the first RenderTexture.
        /// Calls <paramref name="onComplete"/> after the operation finishes.
        /// </summary>
        private void PerformComputePacking(bool finalize, Action<bool> onComplete)
        {
            ChannelBlitParams param = new ChannelBlitParams
            {
                scale = Vector2.one,
                bias = Vector2.zero
            };

            Texture2D[] textures = new Texture2D[4];

            bool performRaster = false;
            for (int i = 0; i < 4; i++)
            {
                var c = workingPreset.contributions[i];

                param.SetChannelData((Channel)i, new ChannelData
                {
                    SrcChannel = c.IsActive ? c.source : Channel.None,
                    SrcTexture = c.IsActive ? (SourceTexture)i : SourceTexture.None
                });

                performRaster |= (!Mathf.Approximately(c.multiply, 1.0f) || c.invert) && c.IsActive;
                textures[i] = c.forceWhite ? Texture2D.whiteTexture : c.texture ?? Texture2D.blackTexture;
            }

            if (!finalize && previewMode != PreviewMode.All)
            {
                int previewIndex = Mathf.Clamp((int)previewMode, 0, 3);
                ChannelData preview = param[(Channel)previewIndex];

                if (preview.IsActive)
                {
                    preview.SrcTexture = (SourceTexture)previewIndex;
                    preview.SrcChannel = (Channel)0;
                }

                for (int i = 0; i < 4; i++)
                {
                    param.SetChannelData((Channel)i,
                        new ChannelData
                        {
                            SrcChannel = (i == 3) ? Channel.None : preview.SrcChannel,
                            SrcTexture = preview.SrcTexture
                        });
                }
            }

            SanitizeInput(ref param, textures);

            Action<AsyncGPUReadbackRequest> invokeRasterPassOrExport = (_) => 
            {
                if (performRaster)
                {
                    onComplete.Invoke(finalize);
                }
                else if (finalize)
                {
                    TextureExporter.ExportRenderTexture(packedTextures.Second, workingPreset.textureFormat);
                }
            };

            ChannelBlitter.ComputeBlit(
                textures[0],
                textures[1],
                textures[2],
                textures[3],
                performRaster ? packedTextures.First : packedTextures.Second,
                param,
                true,
                invokeRasterPassOrExport
            );
        }

        /// <summary>
        /// Performs a rasterization blit of the current workingPreset.contributions into the packed texture.
        /// Exports the final texture if <paramref name="finalize"/> is true.
        /// </summary>
        private void PerformRasterBlit(bool finalize)
        {
            ChannelBlitParams param = new ChannelBlitParams
            {
                scale = Vector2.one,
                bias = Vector2.zero
            };

            for (int i = 0; i < 4; i++)
            {
                var c = workingPreset.contributions[i];

                param.SetChannelData((Channel)i, new ChannelData
                {
                    SrcChannel = (Channel)i,
                    Ops = (c.invert ? ChannelOps.Invert : ChannelOps.None) | ChannelOps.Multiply,
                    Multiplier = c.multiply
                });
            }

            if (!finalize && previewMode != PreviewMode.All)
            {
                int previewIndex = Mathf.Clamp((int)previewMode, 0, 3);
                ChannelData preview = param[(Channel)previewIndex];

                for (int i = 0; i < 4; i++)
                {
                    param.SetChannelData((Channel)i,
                        new ChannelData
                        {
                            SrcChannel = preview.SrcChannel,
                            Ops = preview.Ops,
                            Multiplier = preview.Multiplier
                        });
                }
            }

            ChannelBlitter.RasterBlit(
                packedTextures.First,
                packedTextures.Second,
                param,
                (_) =>
                {
                    if (finalize)
                        TextureExporter.ExportRenderTexture(packedTextures.Second, workingPreset.textureFormat);
                }
            );
        }

        /// <summary>
        /// Ensures at least one channel is active. Fills missing channels with default black texture if needed.
        /// </summary>
        private void SanitizeInput(ref ChannelBlitParams param, Texture2D[] textures)
        {
            bool allEmpty = true;
            for (int i = 0; i < 4; i++)
            {
                var data = param[(Channel)i];
                allEmpty &= !data.IsActive;
            }

            if (allEmpty)
            {
                param.SetChannelData((Channel)0,
                    new ChannelData
                    {
                        SrcChannel = Channel.R,
                        SrcTexture = SourceTexture.Texture0
                    });
                textures[0] = Texture2D.blackTexture;
            }
        }

        /// <summary>
        /// Allocates or recreates a RenderTexture with the specified workingPreset.resolution and format.
        /// </summary>
        private static RenderTexture ReAllocTexture(RenderTexture tex, int resolution, RenderTextureFormat format)
        {
            if (tex == null ||
                tex.width != resolution ||
                tex.height != resolution ||
                tex.format != format)
            {
                if(tex != null)
                    tex.Release();
                tex = new RenderTexture(resolution, resolution, 0, format)
                {
                    enableRandomWrite = true
                };
            }
            if(!tex.IsCreated())
                tex.Create();
            return tex;
        }

        /// <summary>
        /// Ensures the ping-pong textures used for packing are allocated with the correct format and workingPreset.resolution.
        /// </summary>
        private void ReAllocPackerTextures()
        {
            var rtFormat = TextureFormatResolver.ToRenderTextureFormat(workingPreset.textureFormat);

            var tex = packedTextures.First;
            packedTextures.SetFirst(ReAllocTexture(tex, workingPreset.resolution, rtFormat));
            tex = packedTextures.Second;
            packedTextures.SetSecond(ReAllocTexture(tex, workingPreset.resolution, rtFormat));
        }

        private void CleanupRenderTextures()
        {
            if (packedTextures.First != null)
            {
                packedTextures.First.Release();
                DestroyImmediate(packedTextures.First);
            }

            if (packedTextures.Second != null)
            {
                packedTextures.Second.Release();
                DestroyImmediate(packedTextures.Second);
            }

            packedTextures.SetFirst(null);
            packedTextures.SetSecond(null);
        }

        /// <summary>
        /// Clears all channel workingPreset.contributions and resets them to default empty channels.
        /// </summary>
        private void ClearContributions()
        {
            for (int i = 0; i < 4; i++)
                workingPreset.contributions[i] = ChannelContribution.Default(((Channel)i).ToFriendlyString());
        }

        /// <summary>
        /// Loads a preset asset into the editor.
        /// The preset itself remains untouched; instead a hidden working copy
        /// is instantiated so changes can be made without modifying the asset
        /// until explicitly saved.
        /// </summary>
        private void LoadPreset(TexturePackerPreset preset)
        {
            activePreset = preset;

            workingPreset = Instantiate(preset);
            workingPreset.hideFlags = HideFlags.HideAndDontSave;

            var path = AssetDatabase.GetAssetPath(preset);
            EditorPrefs.SetString(LastPresetKey, path);
        }

        /// <summary>
        /// Overwrites the currently active preset asset with the values
        /// from the working preset.
        /// </summary>
        private void SavePreset()
        {
            if (activePreset == null || workingPreset == null)
                return;

            EditorUtility.CopySerialized(workingPreset, activePreset);
            EditorUtility.SetDirty(activePreset);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Saves the current working preset as a new preset asset.
        /// The new asset becomes the active preset afterwards.
        /// </summary>
        private void SavePresetAs()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Texture Packer Preset",
                "NewTexturePackerPreset",
                "asset",
                "Save preset");

            if (string.IsNullOrEmpty(path))
                return;

            var newPreset = CreateInstance<TexturePackerPreset>();
            EditorUtility.CopySerialized(workingPreset, newPreset);

            AssetDatabase.CreateAsset(newPreset, path);
            AssetDatabase.SaveAssets();

            LoadPreset(newPreset);
        }

        /// <summary>
        /// Restores the last active preset from the previous editor session.
        /// This is called when the window is enabled.
        /// </summary>
        private void RestoreLastPreset()
        {
            if (!EditorPrefs.HasKey(LastPresetKey))
                return;

            var path = EditorPrefs.GetString(LastPresetKey);
            var preset = AssetDatabase.LoadAssetAtPath<TexturePackerPreset>(path);

            if (preset != null)
                LoadPreset(preset);
        }

        /// <summary>
        /// Ensures that a working preset instance exists.
        /// This is used as a safety net when no preset has been loaded yet.
        /// </summary>
        private void EnsureWorkingPreset()
        {
            if (workingPreset != null)
                return;

            if (activePreset != null)
            {
                workingPreset = Instantiate(activePreset);
            }
            else
            {
                workingPreset = CreateInstance<TexturePackerPreset>();
            }

            workingPreset.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
