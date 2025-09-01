using UnityEditor;

namespace Editor.SuperUnityBuildCustomActions.Utilities
{
    public static class VersionNames
    {
        public static string GetAndroidVersionName()
        {
            var androidVersionCode = PlayerSettings.Android.bundleVersionCode;
            var bundleVersion = PlayerSettings.bundleVersion;
            var buildType = "Release"; // TODO: get build type from version settings

            var argVersionName = $"v{bundleVersion}-{androidVersionCode}-{buildType}";
            return argVersionName;
        }

        public static string GetIosVersionName()
        {
            var iosVersionCode = PlayerSettings.iOS.buildNumber;
            var bundleVersion = PlayerSettings.bundleVersion;
            var buildType = "Release"; // TODO: get build type from version settings

            var argVersionName = $"v{bundleVersion}-{iosVersionCode}-{buildType}";
            return argVersionName;
        }
    }
}
