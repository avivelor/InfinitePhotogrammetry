using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Unity.XR.MagicLeap.Tests
{
    [AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class RequireMagicLeapDevice : NUnitAttribute, IApplyToTest
    {
        private static string envVariable = "ML_DEVICE_CONNECTED";

        private static string m_SkippedReason =
            String.Format("{0} environment variable not set. Assuming ML device not connected. Skipping test.",
                envVariable);

        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable || test.RunState == RunState.Ignored || IsMagicLeapDeviceConnected())
            {
                return;
            }
            test.RunState = RunState.Skipped;
            test.Properties.Add("_SKIPREASON", m_SkippedReason);
        }

        public static bool IsMagicLeapDeviceConnected()
        {
#if PLATFORM_LUMIN && !UNITY_EDITOR
            return true;
#else
            return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVariable));
#endif // PLATFORM_LUMIN && !UNITY_EDITOR
        }
    }
}