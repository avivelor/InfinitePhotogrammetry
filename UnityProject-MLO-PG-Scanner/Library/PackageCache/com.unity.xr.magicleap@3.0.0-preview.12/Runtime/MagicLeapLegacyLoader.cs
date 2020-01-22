using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.XR.MagicLeap.Remote;
#endif // UNITY_EDITOR
using UnityEngine;
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.MagicLeap
{
    internal sealed class MagicLeapLegacyLoader
    {
        class LoaderComponent : MonoBehaviour
        {
            private List<ISubsystem> m_Subsystems = new List<ISubsystem>();
            private bool m_XRLoaderFound = false;
            void OnDestroy()
            {
                foreach (var subsystem in m_Subsystems)
                    subsystem.Destroy();
                m_Subsystems = null;
            }
            void OnDisable()
            {
                foreach (var subsystem in m_Subsystems)
                    subsystem.Stop();
            }
            void OnEnable()
            {
                StartCoroutine(WaitForDependencies());
            }
            IEnumerator WaitForDependencies()
            {
                while (XRGeneralSettings.Instance == null)
                    yield return null;
                while (XRGeneralSettings.Instance.Manager == null)
                    yield return null;
                m_XRLoaderFound = (XRGeneralSettings.Instance.Manager.ActiveLoaderAs<MagicLeapLoader>() != null);
                if (m_XRLoaderFound)
                    yield break;
                while (!CanCreateSubsystems())
                    yield return null;
#if UNITY_EDITOR
                if (!MagicLeapRemoteManager.Initialize())
                    yield break;
#endif // UNITY_EDITOR
                InitInput();
            }
            public bool CanCreateSubsystems() => !m_XRLoaderFound;

            TSubsystem CreateSubsystemIfNeeded<TSubsystem, TSubsystemDesc>()
                where TSubsystem : class, ISubsystem
                where TSubsystemDesc : ISubsystemDescriptor
            {
                // make sure the subsystem hasn't already been created
                foreach (var subsystem in m_Subsystems)
                {
                    if (subsystem.GetType() is TSubsystem)
                        return (TSubsystem)subsystem;
                }

                var descriptors = new List<TSubsystemDesc>();
                SubsystemManager.GetSubsystemDescriptors<TSubsystemDesc>(descriptors);

                if (descriptors.Count > 0)
                {
                    var descriptorToUse = descriptors[0];
                    if (descriptors.Count > 1)
                    {
                        Debug.LogWarningFormat("Found {0} {1}s. Using \"{2}\"",
                            descriptors.Count, descriptorToUse.GetType(), descriptorToUse.id);
                    }

                    var subsystem = descriptorToUse.Create();
                    if (subsystem == null)
                        return null;
                    m_Subsystems.Add(subsystem);
                    return (TSubsystem)subsystem;
                }

                return null;
            }

            void InitInput()
            {
                var xrInput = CreateSubsystemIfNeeded<XRInputSubsystem, XRInputSubsystemDescriptor>();
                MagicLeapLogger.AssertError(xrInput != null, kLogTag, "Failed to initialized XR Input");
                xrInput.Start();
            }

            public XRMeshSubsystem InitMeshing()
            {
                if (!CanCreateSubsystems())
                    return null;
                var xrMesh = CreateSubsystemIfNeeded<XRMeshSubsystem, XRMeshSubsystemDescriptor>();
                MagicLeapLogger.AssertError(xrMesh != null, kLogTag, "Failed to initialized XR Meshing");
                return xrMesh;
            }
        }
        const string kLogTag = nameof(MagicLeapLegacyLoader);
        private static MagicLeapLegacyLoader s_Instance;
        private LoaderComponent m_Loader;

        public static MagicLeapLegacyLoader instance
        {
            get
            {
                return s_Instance;
            }
        }

        public bool isInitialized
        {
            get { return m_Loader?.CanCreateSubsystems() ?? false; }
        }

        public XRMeshSubsystem meshSubsystem
        {
            get { return m_Loader?.InitMeshing(); }
        }

        private GameObject m_GameObject;

        internal MagicLeapLegacyLoader()
        {
            m_GameObject = new GameObject("MagicLeapLegacyLoader");
            m_GameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Loader = m_GameObject.AddComponent<LoaderComponent>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void InitializeLegacyLoader()
        {
            if (XRSettings.enabled)
                s_Instance = new MagicLeapLegacyLoader();
        }
    }
}