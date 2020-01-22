namespace Tests
{
    using NUnit.Framework;
    using UnityEditor.XR.MagicLeap;
    using UnityEngine;
    using UnityEngine.TestTools;
    public class SDKUtilityTests
    {
        [Test]
        public void PluginAPILevelIsLessThanOrEqualToSDKAPILevel()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            Assert.That(SDKUtility.pluginAPILevel, Is.AtMost(SDKUtility.sdkAPILevel));
        }
    }
}
