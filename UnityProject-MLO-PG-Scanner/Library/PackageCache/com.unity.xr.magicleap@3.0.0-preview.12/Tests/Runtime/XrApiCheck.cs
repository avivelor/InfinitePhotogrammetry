using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.XR;
using System;

namespace Unity.XR.MagicLeap.Tests
{
    public class XrApiCheck : TestBaseSetup
    {
        [Test]
        [RequireMagicLeapDevice]
        public void MobilePlatformCheck()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Assert.IsTrue(Application.isMobilePlatform, "SDK returned as a non mobile platform ");
            }
            else
            {
                Assert.IsFalse(Application.isMobilePlatform, "SDK returned as a mobile platform");
            }
        }

        [Test]
        [RequireMagicLeapDevice]
        public void XrPresentCheck()
        {
            Assert.IsTrue(XRDevice.isPresent, "XR Device is not present");
        }

        [Test]
        [RequireMagicLeapDevice]
        public void UserPresenceCheck()
        {
            if (XRDevice.userPresence == UserPresenceState.Present)
            {
                Assert.AreEqual(UserPresenceState.Present, XRDevice.userPresence,
                    "User Presence reported reported unexpected value");
            }

            if (XRDevice.userPresence == UserPresenceState.NotPresent)
            {
                Assert.AreEqual(UserPresenceState.NotPresent, XRDevice.userPresence,
                    "User Presence reported reported unexpected value");
            }
        }

        [Test]
        [RequireMagicLeapDevice]
        public void XrSettingsCheck()
        {
            Assert.IsTrue(XRSettings.isDeviceActive, "XR Device is not active");
        }

        [Test]
        [RequireMagicLeapDevice]
        public void DeviceCheck()
        {
            Assert.AreEqual(settings.enabledXrTarget, XRSettings.loadedDeviceName, "Wrong XR Device reported");
        }

        // Bug 1141365 is causing the device model to return empty
        [Test]
        [RequireMagicLeapDevice]
        public void XrModel()
        {
            string model = XRDevice.model;
            Assert.IsNotEmpty(model, "Model is empty");
        }

        [Test]
        [RequireMagicLeapDevice]
        public void NativePtr()
        {
            string ptr = XRDevice.GetNativePtr().ToString();
            Assert.IsNotEmpty(ptr, "Native Ptr is empty");
        }

        // Bug 1141366 causes the refresh to return 0
        [Test]
        [RequireMagicLeapDevice]
        public void CheckRefreshRate()
        {
            var refreshRate = XRDevice.refreshRate;

            // Community Manager replied on a forum saying the refresh rate was 60
            Assert.GreaterOrEqual(refreshRate, 60, "Refresh rate returned to lower than expected");
        }
    }
}



