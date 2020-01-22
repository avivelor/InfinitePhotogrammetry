using System.Collections;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.MagicLeap.Rendering;

using NUnit.Framework;

namespace Rendering
{
    public class CameraScaleEditorTests
    {
        [Test]
        public void ConvertingToMLUnitsAtScaleOneIsEqualToOriginalValue()
        {
            Assert.That(RenderingUtility.ToMagicLeapUnits(10f, 1f), Is.EqualTo(10f));
        }

        [Test]
        public void ConvertingToUnityUnitsAtScaleOneIsEqualToOriginalValue()
        {
            Assert.That(RenderingUtility.ToUnityUnits(10f, 1f), Is.EqualTo(10f));
        }

        [Test]
        public void ConvertingToMLUnitsAtScaleTenDividesValueByTen()
        {
            Assert.That(RenderingUtility.ToUnityUnits(1f, 10f), Is.EqualTo(10f));
        }

        [Test]
        public void ConvertingToUnityUnitsAtScaleTenMultipliesValueByTen()
        {
            Assert.That(RenderingUtility.ToMagicLeapUnits(10f, 10f), Is.EqualTo(1f));
        }
    }
}