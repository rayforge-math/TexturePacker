using UnityEditor;

namespace Rayforge.TexturePacker.Editor.Bootstrap
{
    [InitializeOnLoad]
    public static class DependencyBootstrapper
    {
        private static readonly string SessionKey = PackageInfo.CompanyName + "_" + PackageInfo.PackageTitle.Replace(" ", "") + "_DependenciesChecked";

        static DependencyBootstrapper()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            EditorApplication.delayCall += TryOpenInstaller;
        }

        private static void TryOpenInstaller()
        {
            if (!DependencyResolverWindow.AllInstalled())
            {
                DependencyResolverWindow.ShowWindow();
            }
        }
    }
}
