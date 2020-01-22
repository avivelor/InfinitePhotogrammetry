using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools;
using UnityEngine;

namespace Unity.XR.MagicLeap.Tests
{
    public class SpatialMappingTests : TestBaseSetup
    {
        private float kDeviceSetupWait = 1f;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator SpatialMappingWireframeTest()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);
            m_TestSetupHelpers.TestStageSetup(TestStageConfig.MLWireframe);

            yield return null;

            Assert.IsNotNull(m_MLWireFrame, "Wireframe object is null");

            yield return new WaitForSeconds(5f);

            GameObject[] meshObj = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < meshObj.Length; i++)
            {
                if (meshObj[i].name.Contains("Mesh"))
                {
                    Debug.Log("Found Mesh Object : " + meshObj[i]);
                    Assert.IsNotNull(meshObj[i].GetComponent<MeshFilter>().mesh, "Mesh data is null!");
                }
            }
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator SpatialMappingPointCloudTest()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);
            m_TestSetupHelpers.TestStageSetup(TestStageConfig.MLPointCloud);

            yield return null;

            Assert.IsNotNull(m_MLPointCloud, "Wireframe object is null");

            yield return new WaitForSeconds(5f);

            GameObject[] meshObj = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < meshObj.Length; i++)
            {
                if (meshObj[i].name.Contains("Mesh"))
                {
                    Debug.Log("Found Mesh Object : " + meshObj[i]);
                    Assert.IsNotNull(meshObj[i].GetComponent<MeshFilter>().mesh, "Mesh data is null!");
                }
            }
        }
    }
}
