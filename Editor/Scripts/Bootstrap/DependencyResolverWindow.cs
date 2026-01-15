using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Collections.Generic;

namespace Rayforge.TexturePacker.Editor.Bootstrap
{
    public class DependencyResolverWindow : EditorWindow
    {
        private const string WindowTitle = PackageInfo.PackageTitle + " Installer";

        private struct Dependency
        {
            public string name;
            public string packageUrl;
            public string assetPath;
            public bool installed;
            public bool autoInstall;
        }

        private static List<Dependency> _dependencies = new List<Dependency>
        {
            new Dependency
            {
                name = "Scriptable Render Pipeline",
                packageUrl = "com.unity.render-pipelines.core",
                assetPath = "Packages/com.unity.render-pipelines.core",
                installed = false,
                autoInstall = false
            },
            new Dependency
            {
                name = "Rayforge Core Library",
                packageUrl = "https://github.com/rayforge-math/RayforgeUnityLibraryCore.git#1.0.0",
                assetPath = "Packages/com.rayforge.core",
                installed = false,
                autoInstall = true
            }
        };

        private static AddRequest _addRequest;
        private static int _currentInstallingIndex = 0;
        private static readonly string kEditorPrefInstalling = PackageInfo.CompanyName + PackageInfo.PackageTitle.Replace(" ", "") + "_Installing";

        private Texture2D _headerImage;

        [MenuItem("Window/" + PackageInfo.CompanyName + "/" + PackageInfo.PackageTitle + "/Installer")]
        public static void ShowWindow()
        {
            _currentInstallingIndex = 0;
            var window = GetWindow<DependencyResolverWindow>(true, WindowTitle);
            window.minSize = new Vector2(420, 300);
            window.RefreshStatus();
        }

        private void OnEnable()
        {
            _headerImage = Resources.Load<Texture2D>("InstallerHeader");
            RefreshStatus();

            if (EditorPrefs.GetBool(kEditorPrefInstalling, false))
            {
                InstallNextDependency();
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawDescription();
            DrawDependencies();
            DrawFooter();
        }

        private void DrawHeader()
        {
            if (_headerImage == null) return;

            GUILayout.Space(10);

            float aspect = (float)_headerImage.width / _headerImage.height;
            float width = position.width - 20;
            float height = width / aspect;

            GUILayout.Label(_headerImage, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.Space(10);
        }

        private void DrawDescription()
        {
            GUILayout.Label(WindowTitle, EditorStyles.boldLabel);
            GUILayout.Label(
                "This installer sets up required dependencies for " + PackageInfo.PackageTitle + ".",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(12);
        }

        private void DrawDependencies()
        {
            foreach (var dep in _dependencies)
            {
                DrawDependencyStatus(dep.name, dep.installed, dep.assetPath, dep.autoInstall);
            }
        }

        private void DrawFooter()
        {
            GUILayout.Space(10);

            if (_addRequest != null || (_currentInstallingIndex > 0 && _currentInstallingIndex <= _dependencies.Count))
            {
                float progress = 0f;
                string label = "";

                if (_addRequest != null)
                {
                    progress = Mathf.Clamp01((float)_currentInstallingIndex / _dependencies.Count);
                    label = _dependencies[_currentInstallingIndex].name + " (" + Mathf.RoundToInt(progress * 100f) + "%)";
                }
                else if (_currentInstallingIndex > 0 && _currentInstallingIndex <= _dependencies.Count)
                {
                    progress = 1f;
                    label = "Done";
                }

                Rect rect = GUILayoutUtility.GetRect(1, 20);
                EditorGUI.ProgressBar(rect, progress, label);
                GUILayout.Space(6);
            }

            /*
            if (_addRequest != null)
                EditorGUILayout.HelpBox("Installing dependencies…", MessageType.Info);
            */

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(_addRequest != null || AllInstalled()))
            {
                if (GUILayout.Button("Install Dependencies", GUILayout.Height(32)))
                {
                    InstallNextDependency();
                }
            }
        }

        private void DrawDependencyStatus(string name, bool installed, string description, bool autoInstall)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string title = installed ? "✔ " + name : "⚠ " + name;
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);

                // Statustext
                string statusText;
                if (installed)
                    statusText = "Status: Installed";
                else if (autoInstall)
                    statusText = "Status: Missing (will be installed)";
                else
                    statusText = "Status: Missing (optional, not installed)";

                EditorGUILayout.LabelField(statusText, installed ? EditorStyles.miniLabel : EditorStyles.miniBoldLabel);
            }
        }

        internal static bool AllInstalled()
        {
            UpdateDependencyStatus();
            foreach (var dep in _dependencies)
                if (!dep.installed) return false;
            return true;
        }

        private static void UpdateDependencyStatus()
        {
            for (int i = 0; i < _dependencies.Count; i++)
            {
                _dependencies[i] = new Dependency
                {
                    name = _dependencies[i].name,
                    packageUrl = _dependencies[i].packageUrl,
                    assetPath = _dependencies[i].assetPath,
                    installed = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(_dependencies[i].assetPath) != null,
                    autoInstall = _dependencies[i].autoInstall
                };
            }
        }

        private void RefreshStatus()
        {
            UpdateDependencyStatus();
            Repaint();
        }

        private void InstallNextDependency()
        {
            while (_currentInstallingIndex < _dependencies.Count &&
                   (_dependencies[_currentInstallingIndex].installed || !_dependencies[_currentInstallingIndex].autoInstall))
            {
                _currentInstallingIndex++;
            }

            if (_currentInstallingIndex >= _dependencies.Count)
            {
                EditorPrefs.SetBool(kEditorPrefInstalling, false);
                _addRequest = null;
                return;
            }

            EditorPrefs.SetBool(kEditorPrefInstalling, true);
            _addRequest = Client.Add(_dependencies[_currentInstallingIndex].packageUrl);
            EditorApplication.update += InstallProgress;
        }

        private void InstallProgress()
        {
            if (!_addRequest.IsCompleted) return;

            if (_addRequest.Status == StatusCode.Success)
            {
                Debug.Log(_dependencies[_currentInstallingIndex].name + " installed: " + _addRequest.Result.packageId);
            }
            else
            {
                Debug.LogError("Dependency install failed: " + _addRequest.Error.message);
            }

            _addRequest = null;
            EditorApplication.update -= InstallProgress;

            EditorApplication.delayCall += () =>
            {
                RefreshStatus();
                InstallNextDependency();
            };
        }
    }
}