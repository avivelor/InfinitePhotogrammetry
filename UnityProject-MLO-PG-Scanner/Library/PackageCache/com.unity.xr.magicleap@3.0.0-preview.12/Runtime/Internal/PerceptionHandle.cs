using System;
#if PLATFORM_LUMIN
using System.Runtime.InteropServices;
#endif // PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap.Internal
{
    internal struct PerceptionHandle : IDisposable
    {
        static class Native
        {
#if PLATFORM_LUMIN
            const string k_Library = "UnityMagicLeap";

            [DllImport(k_Library, EntryPoint = "UnityMagicLeap_ReleasePerceptionStack")]
            internal static extern void Release(IntPtr ptr);

            [DllImport(k_Library, EntryPoint = "UnityMagicLeap_RetainPerceptionStack")]
            internal static extern IntPtr Retain();
#else
            internal static void Release(IntPtr ptr) {}
            internal static IntPtr Retain() => IntPtr.Zero;
#endif // PLATFORM_LUMIN
        }

        IntPtr m_Handle;

        public bool active
        {
            get { return m_Handle != IntPtr.Zero; }
        }

        internal static PerceptionHandle Acquire()
        {
            return new PerceptionHandle
            {
                m_Handle = Native.Retain()
            };
        }

        public void Dispose()
        {
            if (!active)
                throw new ObjectDisposedException("Handle has already been disposed");

            Native.Release(m_Handle);
            m_Handle = IntPtr.Zero;
        }
    }
}
