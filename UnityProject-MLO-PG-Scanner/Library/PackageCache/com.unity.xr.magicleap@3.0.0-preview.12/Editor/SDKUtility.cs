using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEditor;

namespace UnityEditor.XR.MagicLeap
{
    internal static class SDKUtility
    {
        const string kManifestPath = ".metadata/sdk.manifest";
#if UNITY_EDITOR_WIN
        const string kRemoteLauncher = "VirtualDevice/bin/UIFrontend/MLRemote.exe";
#elif UNITY_EDITOR_OSX
        const string kRemoteLauncher = "VirtualDevice/bin/UIFrontend/Magic Leap Remote.app";
#else
        const string kRemoteLauncher = "Unsupported_on_this_platform.exe";
#endif

        static class Native
        {
            const string Library = "UnityMagicLeap";

            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_PlatformGetAPILevel")]
            public static extern uint GetAPILevel();
        }

        internal static bool isCompatibleSDK
        {
            get
            {
                var min = pluginAPILevel;
                var max = sdkAPILevel;
                return min <= max;
            }
        }
        internal static int pluginAPILevel
        {
            get
            {
                return (int)Native.GetAPILevel();
            }
        }
        internal static bool remoteLauncherAvailable
        {
            get
            {
                if (!sdkAvailable)
                    return false;
                var launcher = Path.Combine(sdkPath, kRemoteLauncher);
#if UNITY_EDITOR_OSX
                return Directory.Exists(launcher);
#else
                return File.Exists(launcher);
#endif
            }
        }
        internal static int sdkAPILevel
        {
            get
            {
                return PrivilegeParser.ParsePlatformLevelFromHeader(Path.Combine(SDKUtility.sdkPath, PrivilegeParser.kPlatformHeaderPath));
            }
        }
        internal static bool sdkAvailable
        {
            get
            {
                if (string.IsNullOrEmpty(sdkPath)) return false;
                return File.Exists(Path.Combine(sdkPath, kManifestPath));
            }
        }
        internal static string sdkPath
        {
            get
            {
                return EditorPrefs.GetString("LuminSDKRoot", null);
            }
        }
    }
}