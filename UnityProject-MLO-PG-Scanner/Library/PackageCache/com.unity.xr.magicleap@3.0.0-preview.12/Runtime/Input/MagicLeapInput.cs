using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Lumin;
using UnityEngine.XR;

namespace UnityEngine.XR.MagicLeap
{
    [AddComponentMenu("AR/Magic Leap/MagicLeap Input")]
    [UsesLuminPrivilege("ControllerPose")]
    public sealed class MagicLeapInput : MonoBehaviour
    {
    }

    public static class MagicLeapInputExtensions
    {
        static class Native
        {
#if PLATFORM_LUMIN
            const string Library = "UnityMagicLeap";

            [DllImport(Library, EntryPoint="UnityMagicLeap_InputGetControllerTrackerActive")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool GetControllerActive();

            [DllImport(Library, EntryPoint="UnityMagicLeap_InputSetControllerTrackerActive")]
            public static extern void SetControllerActive([MarshalAs(UnmanagedType.I1)]bool value);

            [DllImport(Library, EntryPoint="UnityMagicLeap_InputGetEyeTrackerActive")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool GetEyeTrackerActive();

            [DllImport(Library, EntryPoint="UnityMagicLeap_InputSetEyeTrackerActive")]
            public static extern void SetEyeTrackerActive([MarshalAs(UnmanagedType.I1)]bool value);
#else
            public static bool GetControllerActive() => false;
            public static void SetControllerActive(bool value) {}
            public static bool GetEyeTrackerActive() => false;
            public static void SetEyeTrackerActive(bool value) {}

#endif // PLATFORM_LUMIN
        }
        public static bool IsControllerApiEnabled(this XRInputSubsystem self)
        {
            return Native.GetControllerActive();
        }

        public static bool IsEyeTrackingApiEnabled(this XRInputSubsystem self)
        {
            return Native.GetEyeTrackerActive();
        }

        public static void SetControllerApiEnabled(this XRInputSubsystem self, bool enabled)
        {
            Native.SetControllerActive(enabled);
        }

        public static void SetEyeTrackingApiEnabled(this XRInputSubsystem self, bool enabled)
        {
            Native.SetEyeTrackerActive(enabled);
        }
    }

    public static class MagicLeapInputUtility
    {
        public static float[] ParseData(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if ((input.Length % 4) != 0)
                throw new ArgumentException("malformed input array; incorrect number of bytes");

            var list = new List<float>();

            for (int i = 0; i < input.Length; i += 4)
            {
                list.Add(BitConverter.ToSingle(input, i));
            }
            return list.ToArray();
        }
    }
}
