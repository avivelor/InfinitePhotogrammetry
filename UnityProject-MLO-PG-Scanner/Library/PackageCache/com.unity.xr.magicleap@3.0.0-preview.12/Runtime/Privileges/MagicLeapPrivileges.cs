using System.Collections;
using UnityEngine;

#if PLATFORM_LUMIN
using System.Runtime.InteropServices;
#endif // PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    using MLLog = UnityEngine.XR.MagicLeap.MagicLeapLogger;
    public static class MagicLeapPrivileges
    {
        const string kLogTag = "MagicLeapPrivileges";
        enum ResultCode : uint
        {
            Ok = 0,
            UnspecifiedFailure = 4,
            InvalidParameter = 5,
            NotImplemented = 8,
            Granted = 0xcbcd0000,
            Denied = Granted + 1
        }
        static class Native
        {
#if PLATFORM_LUMIN
            const string Library = "ml_privileges";
            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPrivilegesStartup")]
            public static extern ResultCode Startup();

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPrivilegesShutdown")]
            public static extern ResultCode Shutdown();

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPrivilegesCheckPrivilege")]
            public static extern ResultCode CheckPrivilege(uint privilegeId);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPrivilegesRequestPrivilege")]
            public static extern ResultCode RequestPrivilege(uint privilegeId);
#else
            public static ResultCode Startup() { return ResultCode.Ok; }
            public static ResultCode Shutdown() { return ResultCode.Ok; }
            public static ResultCode CheckPrivilege(uint privilegeId) { return (privilegeId != 0) ? ResultCode.Granted : ResultCode.Denied; }
            public static ResultCode RequestPrivilege(uint privilegeId) { return (privilegeId != 0) ? ResultCode.Granted : ResultCode.Denied; }
#endif // PLATFORM_LUMIN
        }

        private static uint s_RefCount = 0;
        private static object s_LockObject = new object();

        private static bool isInitialized() => s_RefCount >= 1;

        internal static void Initialize()
        {
            // TODO :: Need to enfore access on main thread only..
            lock (s_LockObject)
            {
                s_RefCount++;
                if (s_RefCount != 1)
                    return;
            }
            MLLog.Debug(kLogTag, "Starting Privileges Service");
            var result = Native.Startup();
            MLLog.Assert(result == ResultCode.Ok, kLogTag, "Failed to initialize privileges service");
            // on app shutdown, we're tearing the world down anyways, so futzing with the ref count is okay.
            Application.quitting += () => { Shutdown(); };
        }

        internal static void Shutdown()
        {
            // TODO :: Need to enfore access on main thread only..
            lock (s_LockObject)
            {
                s_RefCount--;
                if (s_RefCount > 0)
                    return;
            }
            MLLog.Debug(kLogTag, "Stopping Privileges Service");
            Application.quitting -= Shutdown;
            var result = Native.Shutdown();
            MLLog.Assert(result == ResultCode.Ok, kLogTag, "Failed to shutdown privileges service");
        }

        public static bool IsPrivilegeApproved(uint privilegeId)
        {
            if (!isInitialized()) return false;
            var result = Native.CheckPrivilege(privilegeId);
            MLLog.Debug(kLogTag, "requested id {0}: {1}", privilegeId, result.ToString());
            return result == ResultCode.Granted;
        }

        public static bool RequestPrivilege(uint privilegeId)
        {
            if (!isInitialized()) return false;
            var result = Native.RequestPrivilege(privilegeId);
            MLLog.Debug(kLogTag, "requested id {0}: {1}", privilegeId, result.ToString());
            return result == ResultCode.Granted;
        }
    }

    // Helper class for pausing script execution until
    // a specific permission is available.
    internal sealed class WaitForPrivilege : IEnumerator
    {
        private readonly uint m_PrivilegeId;
        private readonly System.Func<IEnumerator> m_YieldFunc;
        public WaitForPrivilege(uint privilegeId) : this(privilegeId, null)
        {
        }

        public WaitForPrivilege(uint privilegeId, System.Func<IEnumerator> yieldFunc)
        {
            m_PrivilegeId = privilegeId;
            m_YieldFunc = yieldFunc;
        }
        object IEnumerator.Current => (m_YieldFunc != null) ? m_YieldFunc() : null;

        bool IEnumerator.MoveNext() => !MagicLeapPrivileges.IsPrivilegeApproved(m_PrivilegeId);

        void IEnumerator.Reset() {}
    }
}