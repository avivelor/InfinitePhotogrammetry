namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using UnityEditor.XR.MagicLeap;

    public class PlatformLevelSelectorTests
    {
        [Test]
        public void VerifyPlatformLevelBelowMinimumIsSetToMinimum()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            var current = PlatformLevelSelector.GetChoices().Min() - 1;
            Assert.That(PlatformLevelSelector.EnsureValidValue(current), Is.EqualTo(PlatformLevelSelector.GetChoices().Min()));
        }

        [Test]
        public void VerifyPlatformLevelAboveMaximumIsSetToMaximum()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            var current = PlatformLevelSelector.GetChoices().Max() + 1;
            Assert.That(PlatformLevelSelector.EnsureValidValue(current), Is.EqualTo(PlatformLevelSelector.GetChoices().Max()));
        }

        [Test]
        public void VerifyPlatformLevelAtMinimumIsUnchanged()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            var min = PlatformLevelSelector.GetChoices().Min();
            Assert.That(PlatformLevelSelector.EnsureValidValue(min), Is.EqualTo(min));
        }

        [Test]
        public void VerifyPlatformLevelAtMaximumIsUnchanged()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            var max = PlatformLevelSelector.GetChoices().Max();
            Assert.That(PlatformLevelSelector.EnsureValidValue(max), Is.EqualTo(max));
        }
    }
}