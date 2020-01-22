using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR;
using Assert = UnityEngine.Assertions.Assert;
using Unity.XR.MagicLeap.Tests;

[UnityPlatform(include = new[] { RuntimePlatform.Lumin })]
public class LuminSmokeTest : TestBaseSetup
{
    [UnityTest]
    [RequireMagicLeapDevice]
    [Explicit] // Added to ensure this is only run against Magic Leap Devices
               // Requires that --testFilter=LuminSmokeTest parameter is used to run.
    public IEnumerator CanDeployAndRunOnLuminDevice()
    {
        yield return new MonoBehaviourTest<LuminMonoBehaviourTest>();
    }
}

public class LuminMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
{
    public bool IsTestFinished { get; private set; }

    void Awake()
    {
        Assert.IsTrue(XRSettings.enabled);
        Assert.AreEqual("Lumin", XRSettings.loadedDeviceName);
        IsTestFinished = true;
    }
}
