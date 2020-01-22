using System;
using System.IO;

using UnityEngine;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.MagicLeap
{
    internal class DisabledAttribute : PropertyAttribute
    {
        public DisabledAttribute() {}
    }

    [Serializable]
    [XRConfigurationData("Magic Leap Settings", MagicLeapConstants.kSettingsKey)]
    public class MagicLeapSettings : ScriptableObject
    {
        [Serializable]
        public class GLCache
        {
            [SerializeField, Tooltip("Select to optimize application and use cached shader data.")]
            bool m_Enabled;

            [SerializeField, Tooltip("The maximium size for each shader blob data, units in bytes.")]
            uint m_MaxBlobSizeInBytes ;

            [SerializeField, Tooltip("The maximium size for shader cache file, units in bytes.")]
            uint m_MaxFileSizeInBytes;

            public bool enabled
            {
                get { return m_Enabled; }
                set { m_Enabled = value; }
            }

            public uint maxBlobSizeInBytes
            {
                get { return m_MaxBlobSizeInBytes; }
                set { m_MaxBlobSizeInBytes = value; }
            }

            public uint maxFileSizeInBytes
            {
                get { return m_MaxFileSizeInBytes; }
                set { m_MaxFileSizeInBytes = value; }
            }

            internal string cachePath => Path.Combine(Application.persistentDataPath, "blob_cache.dat");
        }

    #if !UNITY_EDITOR
        static MagicLeapSettings s_RuntimeInstance = null;
    #endif // !UNITY_EDITOR

        public static MagicLeapSettings currentSettings
        {
            get
            {
                MagicLeapSettings settings = null;
            #if UNITY_EDITOR
                UnityEditor.EditorBuildSettings.TryGetConfigObject(MagicLeapConstants.kSettingsKey, out settings);
            #else
                settings = s_RuntimeInstance;
            #endif // UNITY_EDITOR
                return settings;
            }
        }

        [SerializeField, Tooltip("Defines the precision of the depth buffers; higher values allow a wider range of values, but are usually slower")]
        Rendering.DepthPrecision m_DepthPrecision;

        [SerializeField, Tooltip("Force Multipass rendering. Select this option when shaders are incompatible with Single Pass Instancing")]
        bool m_ForceMultipass;

        [SerializeField, Tooltip("Defines the minimum frame time interval, or the maximum speed at which the system will process frames")]
        Rendering.FrameTimingHint m_FrameTimingHint = Rendering.FrameTimingHint.Max_60Hz;

        [SerializeField, Tooltip("Allows OpenGLES shaders to be cached on device, saving compilation time on subsequent runs")]
        GLCache m_GLCacheSettings;

        [SerializeField, Tooltip("Enables gesture subsystem, allowing for the detection of touch and hand gestures")]
        bool m_EnableGestures;

        public Rendering.DepthPrecision depthPrecision
        {
            get { return m_DepthPrecision; }
            set { m_DepthPrecision = value; }
        }

        public bool forceMultipass
        {
            get { return m_ForceMultipass; }
            set { m_ForceMultipass = value; }
        }

        public Rendering.FrameTimingHint frameTimingHint
        {
            get { return m_FrameTimingHint; }
            set { m_FrameTimingHint = value; }
        }

        public GLCache glCacheSettings
        {
            get { return m_GLCacheSettings; }
            internal set { m_GLCacheSettings = value; }
        }

        public bool enableGestures
        {
            get { return m_EnableGestures; }
            internal set { m_EnableGestures = value; }
        }

        void Awake()
        {
            #if !UNITY_EDITOR
            s_RuntimeInstance = this;
            #endif // !UNITY_EDITOR
        }
    }
}