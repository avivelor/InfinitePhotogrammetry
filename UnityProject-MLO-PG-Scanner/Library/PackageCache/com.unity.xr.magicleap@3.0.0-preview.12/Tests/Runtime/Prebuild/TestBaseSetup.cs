using NUnit.Framework;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.TestTools;
using System;
using System.Collections;
using UnityEngine.XR;

#if UNITY_EDITOR
using UnityEditor;
#endif

[PrebuildSetup("EnablePlatformPrebuildStep")]
public class TestBaseSetup
{
    public CurrentSettings settings;

    public static Camera m_Camera;
            
    public static GameObject m_Light;
    public static GameObject m_Cube;
            
    public static GameObject m_XrManager;
    public static GameObject m_TrackingRig;
    public static GameObject m_MLWireFrame;
    public static GameObject m_MLPointCloud;
    public static TrackedPoseDriver m_TrackHead;

    public static TestSetupHelpers m_TestSetupHelpers;


    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        settings = Resources.Load<CurrentSettings>("settings");
    }


    [SetUp]
    public virtual void SetUp()
    {
        m_TestSetupHelpers = new TestSetupHelpers();

        m_TestSetupHelpers.TestStageSetup(TestStageConfig.BaseStageSetup);
    }

    [TearDown]
    public virtual void TearDown()
    {
        m_TestSetupHelpers.TestStageSetup(TestStageConfig.CleanStage);
    }

    [UnitySetUp]
    public IEnumerator SetUpAndEnableXR()
    {
        if (XRSettings.loadedDeviceName != settings.enabledXrTarget)
        {
            XRSettings.LoadDeviceByName(settings.enabledXrTarget);
        }

        yield return null;

        XRSettings.enabled = true;
    }
}
