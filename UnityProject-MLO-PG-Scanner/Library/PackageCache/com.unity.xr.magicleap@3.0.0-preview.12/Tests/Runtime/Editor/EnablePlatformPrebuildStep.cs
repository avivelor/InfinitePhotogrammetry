using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using NDesk.Options;

public class EnablePlatformPrebuildStep : IPrebuildSetup
{
    private const string kLuminSdkEnvironment = "LUMINSDK_UNITY";
    public void Setup()
    {
        var args = System.Environment.GetCommandLineArgs();

        PlatformSettings.enabledXrTargets = new string[] { "None" };

        if (args.Length <= 1)
        {
            switch (EditorUserBuildSettings.selectedBuildTargetGroup)
            {
                case BuildTargetGroup.Standalone:
                    PlatformSettings.enabledXrTargets = new string[] { "MockHMD", "None" };
                    PlatformSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
                    PlatformSettings.playerGraphicsApi =
                        (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
                            ? GraphicsDeviceType.Direct3D11
                            : GraphicsDeviceType.OpenGLCore;
                    PlatformSettings.mtRendering = true;
                    PlatformSettings.graphicsJobs = false;
                    break;
                case BuildTargetGroup.WSA:
                    // Configure WSA build
                    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WSAPlayer && EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.WSA)
                    {
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                    }
                    EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
                    EditorUserBuildSettings.wsaSubtarget = WSASubtarget.AnyDevice;
                    EditorUserBuildSettings.allowDebugging = true;

                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.WSA, ScriptingImplementation.IL2CPP);
                    PlatformSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;

                    PlatformSettings.enabledXrTargets = new string[] { "WindowsMR", "None" };
                    break;
                case BuildTargetGroup.Android:
                case BuildTargetGroup.iOS:
                    PlatformSettings.enabledXrTargets = new string[] { "cardboard", "None" };
                    PlatformSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
                    PlatformSettings.playerGraphicsApi = GraphicsDeviceType.OpenGLES3;
                    break;
                case BuildTargetGroup.Lumin:
                    PlatformSettings.enabledXrTargets = new string[] { "Lumin", "None" };
                    var sdk = Environment.GetEnvironmentVariable(kLuminSdkEnvironment);
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Lumin, BuildTarget.Lumin);
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Lumin, "com.unity.luminplaymodetest");
                    PlayerSettings.virtualRealitySupported = true;
                    UnityEditorInternal.VR.VREditor.SetVirtualRealitySDKs(BuildTargetGroup.Lumin, new[] { "Lumin" });
                    if (!string.IsNullOrEmpty(sdk))
                    {
                        EditorPrefs.SetString("LuminSDKRoot", sdk);
                    }
                    break;
            }
        }
        else
        {
            var optionSet = DefineOptionSet();
            var unprocessedArgs = optionSet.Parse(args);
        }

        ConfigureSettings();
        
        PlatformSettings.SerializeToAsset();

    }

    private void ConfigureSettings()
    {
        PlayerSettings.virtualRealitySupported = true;

        UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(
            PlatformSettings.BuildTargetGroup,
            PlatformSettings.enabledXrTargets);

        if (PlatformSettings.enabledXrTargets.FirstOrDefault() != "WindowsMR")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                PlatformSettings.BuildTargetGroup,
                PlatformSettings.BuildTarget);
        }

        PlayerSettings.stereoRenderingPath = PlatformSettings.stereoRenderingPath;

        PlayerSettings.Android.minSdkVersion = PlatformSettings.minimumAndroidSdkVersion;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Development;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
    }

    private static OptionSet DefineOptionSet()
    {
        return new OptionSet()
            {
                {
                    "enabledxrtarget=",
                    "XR target to enable in player settings. Values: \r\n\"Oculus\"\r\n\"OpenVR\"\r\n\"cardboard\"\r\n\"daydream\"\r\n\"MockHMD\"\r\n\"Lumin\"",
                    xrTarget => PlatformSettings.enabledXrTargets = new string[] {xrTarget, "None"}
                },
                {
                    "simulationMode=",
                    "Enable Simulation modes for Windows MR in Editor. Values: \r\n\"HoloLens\"\r\n\"WindowsMR\"\r\n\"Remoting\"\r\n\"Lumin\"",
                    simMode => PlatformSettings.simulationMode = simMode
                },
                {
                    "playergraphicsapi=", "Graphics API based on GraphicsDeviceType.",
                    graphicsDeviceType => PlatformSettings.playerGraphicsApi =
                        TryParse<GraphicsDeviceType>(graphicsDeviceType)
                },
                {
                    "stereorenderingpath=", "Stereo rendering path to enable. SinglePass is default",
                    stereoRenderingPath => PlatformSettings.stereoRenderingPath =
                        TryParse<StereoRenderingPath>(stereoRenderingPath)
                },
                {
                    "mtrendering", "Use multi threaded rendering; true is default.",
                    gfxMultithreaded =>
                    {
                        if (gfxMultithreaded.ToLower() == "true")
                        {
                            PlatformSettings.mtRendering = true;
                            PlatformSettings.graphicsJobs = false;
                        }
                    }
                },
                {
                    "graphicsjobs", "Use graphics jobs rendering; false is default.",
                    gfxJobs =>
                    {
                        if (gfxJobs.ToLower() == "true")
                        {
                            PlatformSettings.mtRendering = false;
                            PlatformSettings.graphicsJobs = true;
                        }
                    }
                },
                {
                    "minimumandroidsdkversion=", "Minimum Android SDK Version to use.",
                    minAndroidSdkVersion => PlatformSettings.minimumAndroidSdkVersion =
                        TryParse<AndroidSdkVersions>(minAndroidSdkVersion)
                },
                {
                    "targetandroidsdkversion=", "Target Android SDK Version to use.",
                    targetAndroidSdkVersion => PlatformSettings.targetAndroidSdkVersion =
                        TryParse<AndroidSdkVersions>(targetAndroidSdkVersion)
                }
            };
    }

    private static T TryParse<T>(string stringToParse)
    {
        T thisType;
        try
        {
            thisType = (T)Enum.Parse(typeof(T), stringToParse);
        }
        catch (Exception e)
        {
            throw new ArgumentException(($"Couldn't cast {stringToParse} to {typeof(T)}"), e);
        }

        return thisType;
    }

    private static string[] ParseMultipleArgs(string args)
    {
        return args.Split(';');
    }

    private BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneOSX:
                {
                    return BuildTargetGroup.Standalone;
                }
            case BuildTarget.Android:
                {
                    return BuildTargetGroup.Android;
                }
            case BuildTarget.iOS:
                {
                    return BuildTargetGroup.iOS;
                }
            case BuildTarget.WSAPlayer:
                {
                    return BuildTargetGroup.WSA;
                }
            case BuildTarget.Lumin:
                {
                    return BuildTargetGroup.Lumin;
                }
            default:
                {
                    Debug.LogError("Unsupported build target.");
                    return BuildTargetGroup.Standalone;
                }
        }
    }
}

