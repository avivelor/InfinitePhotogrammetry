#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.XR;

using AOT;

namespace UnityEditor.XR.MagicLeap.Remote
{
    public class MagicLeapRemoteManager
    {
        static class Native
        {
            const string k_Library = "UnityMagicLeap";
            [DllImport(k_Library, EntryPoint = "UnityMagicLeap_RemoteInitialize")]
            [return: MarshalAs(UnmanagedType.I1)]
            internal static extern bool RemoteInitialize();
        }

        public delegate bool RemoteLoaderCallback([MarshalAs(UnmanagedType.LPStr)] string libName, out IntPtr handle);

        private static Dictionary<string, string> s_PluginLookupCache = new Dictionary<string, string>();

        [Obsolete("This property has been deprecated and will be removed in a future release")]
        public static bool isInitialized => true;

        [Obsolete("This method has been deprecated and will be removed in a future release")]
        public static void InitializeWithLoaderCallback(RemoteLoaderCallback cb = null) {}

        [Obsolete("This method has been deprecated and will be removed in a future release")]
        public static void SetLoaderCallback(RemoteLoaderCallback cb = null) {}

        public static bool Initialize() => Native.RemoteInitialize();

        public static void Shutdown() {}
    }
}
#endif // UNITY_EDITOR