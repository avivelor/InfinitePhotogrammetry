
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityEngine.XR.MagicLeap.Rendering
{
    public enum DepthPrecision : int
    {
        Depth32,
        Depth24Stencil8
    }

    public enum FrameTimingHint : int
    {
        Unspecified = 0,
        Maximum,
        Max_60Hz,
        Max_120Hz,
    }

    public enum StabilizationMode : byte
    {
        None,
        FarClip,
        FurthestObject,
        Custom
    }
    public static class RenderingSettings
    {
        const float kDefaultFarClip = 10f;
        const float kDefaultNearClip = 5f;
        // TODO / FIXME :: All the string marshalling being done here is probably sub-optimal,
        // but it needs to be profiled first.

        // All values here are expected to be in Unity units, but need to be stored in
        // in MagicLeap units (meters), so we do all the conversion here to keep logic
        // elsewhere as simple as possible.
        internal static float s_CachedCameraScale = 1.0f;
        public static float cameraScale
        {
            get
            {
                return (UnityMagicLeap_RenderingTryGetParameter("CameraScale", out s_CachedCameraScale)) ? s_CachedCameraScale : 1f;
            }
            internal set
            {
                s_CachedCameraScale = value;
                UnityMagicLeap_RenderingSetParameter("CameraScale", s_CachedCameraScale);
            }
        }
        internal static DepthPrecision depthPrecision
        {
            get
            {
                return UnityMagicLeap_RenderingGetDepthPrecision();
            }

            set
            {
                UnityMagicLeap_RenderingSetDepthPrecision(value);
            }
        }
        public static float farClipDistance
        {
            get
            {
                float farClip = kDefaultFarClip;
                UnityMagicLeap_RenderingTryGetParameter("FarClipDistance", out farClip);
                return RenderingUtility.ToUnityUnits(farClip, s_CachedCameraScale);
            }
            internal set { UnityMagicLeap_RenderingSetParameter("FarClipDistance", RenderingUtility.ToMagicLeapUnits(value, s_CachedCameraScale)); }
        }
        public static float focusDistance
        {
            get
            {
                float focus = 0f;
                UnityMagicLeap_RenderingTryGetParameter("FocusDistance", out focus);
                return RenderingUtility.ToUnityUnits(focus, s_CachedCameraScale); 
            }
            internal set { UnityMagicLeap_RenderingSetParameter("FocusDistance", RenderingUtility.ToMagicLeapUnits(value, s_CachedCameraScale)); }
        }
        public static FrameTimingHint frameTimingHint
        {
            get { return UnityMagicLeap_RenderingGetFrameTimingHint(); }
            internal set { UnityMagicLeap_RenderingSetFrameTimingHint(value); }
        }
        public static float maxFarClipDistance
        {
            get
            {
                float maxFarClip = float.PositiveInfinity;
                UnityMagicLeap_RenderingTryGetParameter("MaxFarClipDistance", out maxFarClip);
                return RenderingUtility.ToUnityUnits(maxFarClip, s_CachedCameraScale);
            }
        }
        [Obsolete("use minNearClipDistance instead")]
        public static float maxNearClipDistance
        {
            get { return minNearClipDistance;}
        }
        public static float minNearClipDistance
        {
            get
            {
                float minNearClip = 0.5f;
                UnityMagicLeap_RenderingTryGetParameter("MinNearClipDistance", out minNearClip);
                return RenderingUtility.ToUnityUnits(minNearClip, s_CachedCameraScale);
            }
        }
        public static float nearClipDistance
        {
            get
            {
                float nearClip = 0.5f;
                UnityMagicLeap_RenderingTryGetParameter("NearClipDistance", out nearClip);
                return RenderingUtility.ToUnityUnits(nearClip, s_CachedCameraScale);
            }
            internal set { UnityMagicLeap_RenderingSetParameter("NearClipDistance", RenderingUtility.ToMagicLeapUnits(value, s_CachedCameraScale)); }
        }
        [Obsolete("use MagicLeapSettings.forceMultipass to force multipass rendering instead")]
        public static bool singlePassEnabled
        {
            get
            {
                float enabled = 0.0f;
                UnityMagicLeap_RenderingTryGetParameter("SinglePassEnabled", out enabled);
                return IsFlagSet(enabled);
            }
            internal set { UnityMagicLeap_RenderingSetParameter("SinglePassEnabled", value ? 1.0f : 0.0f); }
        }
        public static float stabilizationDistance
        {
            get
            {
                float distance = 10f;
                UnityMagicLeap_RenderingTryGetParameter("StabilizationDistance", out distance);
                return RenderingUtility.ToUnityUnits(distance, s_CachedCameraScale);
            }
            internal set { UnityMagicLeap_RenderingSetParameter("StabilizationDistance", RenderingUtility.ToMagicLeapUnits(value, s_CachedCameraScale)); }
        }
        public static bool useProtectedSurface
        {
            get
            {
                float enabled = 0f;
                UnityMagicLeap_RenderingTryGetParameter("UseProtectedSurface", out enabled);
                return IsFlagSet(enabled);
            }
            internal set { UnityMagicLeap_RenderingSetParameter("UseProtectedSurface", value ? 1.0f : 0.0f); }
        }
        [Obsolete("Use UnityEngine.XR.XRSettings.renderViewportScale instead")]
        public static float surfaceScale
        {
            get
            {
                float scale = 1f;
                UnityMagicLeap_RenderingTryGetParameter("SurfaceScale", out scale);
                return scale;
            }
            internal set { UnityMagicLeap_RenderingSetParameter("SurfaceScale", value); }
        }
        [Obsolete("useLegacyFrameParameters is ignored on XR SDK")]
        internal static bool useLegacyFrameParameters
        {
            get
            {
                float enabled = 0f;
                UnityMagicLeap_RenderingTryGetParameter("UseLegacyFrameParameters", out enabled);
                return IsFlagSet(enabled);
            }
            set { UnityMagicLeap_RenderingSetParameter("UseLegacyFrameParameters", value ? 1.0f : 0.0f); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFlagSet(float val)
        {
            return Mathf.Approximately(val, 0f);
        }

#if PLATFORM_LUMIN
        const string kLibrary = "UnityMagicLeap";
        [DllImport(kLibrary)]
        internal static extern FrameTimingHint UnityMagicLeap_RenderingGetFrameTimingHint();
        [DllImport(kLibrary)]
        internal static extern void UnityMagicLeap_RenderingSetFrameTimingHint(FrameTimingHint newValue);
        [DllImport(kLibrary, CharSet = CharSet.Ansi)]
        internal static extern void UnityMagicLeap_RenderingSetParameter(string key, float newValue);
        [DllImport(kLibrary, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool UnityMagicLeap_RenderingTryGetParameter(string key, out float value);
        [DllImport(kLibrary)]
        internal static extern DepthPrecision UnityMagicLeap_RenderingGetDepthPrecision();
        [DllImport(kLibrary)]
        internal static extern void UnityMagicLeap_RenderingSetDepthPrecision(DepthPrecision depthPrecision);
#else
        internal static FrameTimingHint UnityMagicLeap_RenderingGetFrameTimingHint() { return FrameTimingHint.Unspecified; }
        internal static void UnityMagicLeap_RenderingSetFrameTimingHint(FrameTimingHint newValue) {}
        internal static void UnityMagicLeap_RenderingSetParameter(string key, float newValue) {}
        internal static bool UnityMagicLeap_RenderingTryGetParameter(string key, out float value) { value = 0f; return false; }
        internal static DepthPrecision UnityMagicLeap_RenderingGetDepthPrecision() { return DepthPrecision.Depth32; }
        internal static void UnityMagicLeap_RenderingSetDepthPrecision(DepthPrecision depthPrecision) {}
#endif

        // device-specific calls.
#if PLATFORM_LUMIN && !UNITY_EDITOR
        [DllImport("libc", EntryPoint = "__system_property_get")]
        private static extern int _GetSystemProperty(string name, StringBuilder @value);

        public static string GetSystemProperty(string name)
        {
            var sb = new StringBuilder(255);
            var ret = _GetSystemProperty(name, sb);
            return ret == 0 ? sb.ToString() : null;
        }
#endif
    }
}
