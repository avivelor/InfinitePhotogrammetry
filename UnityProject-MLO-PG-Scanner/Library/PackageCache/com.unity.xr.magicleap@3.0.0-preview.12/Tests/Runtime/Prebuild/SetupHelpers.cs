using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SpatialTracking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TestStageConfig
{
    BaseStageSetup,
    CleanStage,
    MultiPass,
    Instancing,
    MLWireframe,
    MLPointCloud
}

public enum TestCubesConfig
{
    None,
    TestCube,
    PerformanceMassFloorObjects,
    PerformanceMassObjects,
    TestMassCube
}

public class TestSetupHelpers : TestBaseSetup
{
    private int m_CubeCount = 0;

    private void CreateXRGameObjects()
    {
        m_TrackingRig = GameObject.Instantiate(Resources.Load("TestSetup/TrackingRig")) as GameObject;
    }

    private void CreateMLSpaitalWireFrame()
    {
        m_MLWireFrame = GameObject.Instantiate(Resources.Load("TestSetup/SpatialMappingWireFrame")) as GameObject;
    }

    private void CreateMLPointCloud()
    {
        m_MLPointCloud = GameObject.Instantiate(Resources.Load("TestSetup/SpatialMappingPointCloud")) as GameObject;
    }

    private void CameraLightSetup()
    {
        m_Camera = m_TrackingRig.GetComponentInChildren<Camera>();
        m_Camera.tag = "MainCamera";

        m_Light = new GameObject("Light");
        Light light = m_Light.AddComponent<Light>();
        light.type = LightType.Directional;
    }

    private void TestCubeCreation()
    {
        m_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_Cube.transform.position = 5f * Vector3.forward;
    }

    private void CreateMassFloorObjects()
    {
        float x = -3.0f;
        float y = -0.5f;
        float zRow1 = 2.0f;
        float zRow2 = 2.0f;
        float zRow3 = 2.0f;
        float zRow4 = 2.0f;

        for (int i = 0; i < 20; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CubeCount += 1;
            obj.name = "TestCube " + m_CubeCount;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.transform.localPosition = new Vector3(x, y, zRow1);

            zRow1 = zRow1 + 0.5f;
            x = -2f;
        }

        for (int i = 0; i < 20; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CubeCount += 1;
            obj.name = "TestCube " + m_CubeCount;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.transform.localPosition = new Vector3(x, y, zRow2);

            zRow2 = zRow2 + 0.5f;
            x = -1f;
        }

        for (int i = 0; i < 20; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CubeCount += 1;
            obj.name = "TestCube " + m_CubeCount;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.transform.localPosition = new Vector3(x, y, zRow3);

            zRow3 = zRow3 + 0.5f;
            x = 0f;
        }

        for (int i = 0; i < 20; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CubeCount += 1;
            obj.name = "TestCube " + m_CubeCount;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.transform.localPosition = new Vector3(x, y, zRow4);

            zRow4 = zRow4 + 0.5f;
            x = 1f;
        }
    }

    private void CreateMassObjects()
    {
        float xRow1 = -0.5f;
        float xRow2 = -0.5f;
        float xRow3 = -0.5f;
        float yRow1 = 0.2f;
        float yRow2 = -0.2f;
        float yRow3 = -0.01f;
        float zRow1 = 2.5f;
        float zRow2 = 2.3f;

        for (int i = 0; i < 17; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CubeCount += 1;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.transform.localPosition = new Vector3(xRow1, yRow1, zRow1);
            obj.name = "TestCube " + m_CubeCount;

            if (i < 5)
            {
                xRow1 = xRow1 + 0.2f;
                obj.transform.localPosition = new Vector3(xRow1, yRow1, zRow1);
            }
            else if (i > 5 && i < 11)
            {
                xRow2 = xRow2 + 0.2f;
                obj.transform.localPosition = new Vector3(xRow2, yRow2, zRow1);
            }
            else if (i > 11)
            {
                xRow3 = xRow3 + 0.2f;
                obj.transform.localPosition = new Vector3(xRow3, yRow3, zRow2);
            }
        }
    }

    private void CleanUpTestCubes()
    {
        if (m_CubeCount > 0)
        {
            for (int i = 0; i < m_CubeCount + 1; i++)
            {
                var obj = GameObject.Find("TestCube " + i);
                Object.Destroy(obj);
            }
        }

        if (GameObject.Find("Cube"))
        {
            Object.Destroy(GameObject.Find("Cube"));
        }
    }

    private void DestroyGameObject(ref GameObject obj)
    {
        if (obj != null)
        {
            Object.Destroy(obj);
            obj = null;
        }
    }

    private void CleanUpCameraLights()
    {
        DestroyGameObject(ref m_TrackingRig);
        DestroyGameObject(ref m_Light);
    }

    private void CleanUpXRGameObjects()
    {
        DestroyGameObject(ref m_XrManager);
        DestroyGameObject(ref m_TrackingRig);
        if (m_MLPointCloud != null)
        {
            DestroyGameObject(ref m_MLPointCloud);

            GameObject[] meshObj = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < meshObj.Length; i++)
            {
                if (meshObj[i].name.Contains("Mesh"))
                {
                    DestroyGameObject(ref meshObj[i]);
                }
            }
        }

        if (m_MLWireFrame != null)
        {
            DestroyGameObject(ref m_MLWireFrame);

            GameObject[] meshObj = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < meshObj.Length; i++)
            {
                if (meshObj[i].name.Contains("Mesh"))
                {
                    DestroyGameObject(ref meshObj[i]);
                }
            }
        }
    }

#if UNITY_EDITOR
    public void EnsureMultiPassRendering() => PlayerSettings.stereoRenderingPath = StereoRenderingPath.MultiPass;
                
    public void EnsureInstancingRendering() => PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
#endif

    public void TestStageSetup(TestStageConfig TestConfiguration)
    {
        switch (TestConfiguration)
        {
            case TestStageConfig.BaseStageSetup:
                    CreateXRGameObjects();
                    CameraLightSetup();
                    break;

            case TestStageConfig.CleanStage:
                    CleanUpCameraLights();
                    CleanUpTestCubes();
                    CleanUpXRGameObjects();
                    break;
            case TestStageConfig.MLWireframe:
                    CreateMLSpaitalWireFrame();
                    break;
            case TestStageConfig.MLPointCloud:
                    CreateMLPointCloud();
                    break;
#if UNITY_EDITOR
            case TestStageConfig.Instancing:
                    EnsureInstancingRendering();
                    break;

                case TestStageConfig.MultiPass:
                    EnsureMultiPassRendering();
                    break;
#endif
        }
    }

    public void TestCubeSetup(TestCubesConfig TestConfiguration)
    {
        switch (TestConfiguration)
        {
            case TestCubesConfig.TestCube:
                TestCubeCreation();
                break;

            case TestCubesConfig.PerformanceMassFloorObjects:
                CreateMassFloorObjects();
                break;

            case TestCubesConfig.PerformanceMassObjects:
                CreateMassObjects();
                break;

            case TestCubesConfig.None:
                break;
        }
    }
}
